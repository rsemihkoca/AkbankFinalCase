using Application.Cqrs;
using Application.Services;
using Application.Validators;
using AutoMapper;
using Business.Entities;
using Business.Enums;
using Hangfire;
using Infrastructure.Data.DbContext;
using Infrastructure.Dtos;
using MediatR;

namespace Application.Commands;

public class ExpenseCommandHandler :
    IRequestHandler<CreateExpenseCommand, ExpenseResponse>,
    IRequestHandler<UpdateExpenseCommand, ExpenseResponse>,
    IRequestHandler<DeleteExpenseCommand, ExpenseResponse>,
    IRequestHandler<ApproveExpenseCommand, ExpenseResponse>,
    IRequestHandler<RejectExpenseCommand, ExpenseResponse>
{
    private readonly ExpenseDbContext dbContext;
    private readonly IMapper mapper;
    private readonly IHandlerValidator validate;
    private readonly IPaymentService payment;

    public ExpenseCommandHandler(ExpenseDbContext dbContext, IMapper mapper, IHandlerValidator validator, IPaymentService payment)
    {
        this.dbContext = dbContext;
        this.mapper = mapper;
        this.validate = validator;
        this.payment = payment;
    }


    public async Task<ExpenseResponse> Handle(CreateExpenseCommand request, CancellationToken cancellationToken)
    {
        (string role, int creatorId) = await validate.UserAuthAsync(request.Model.UserId, cancellationToken);

        await validate.RecordExistAsync<User>(x => x.UserId == request.Model.UserId, cancellationToken);
        await validate.RecordExistAsync<ExpenseCategory>(x => x.CategoryId == request.Model.CategoryId,
            cancellationToken);

        var entity = mapper.Map<CreateExpenseRequest, Expense>(request.Model);
        entity.CreatedBy = creatorId;

        var entityResult = await dbContext.AddAsync(entity, cancellationToken);
        await dbContext.SaveChangesAsync(cancellationToken);

        var mapped = mapper.Map<Expense, ExpenseResponse>(entityResult.Entity);
        return mapped;
    }

    public async Task<ExpenseResponse> Handle(UpdateExpenseCommand request, CancellationToken cancellationToken)
    {
        // admin kendini approve edemez basşka bir admin approve edebilir ? Bundan vazgectim
        // Update approved bir sey tetiklemiyor sadece update ediyor elden verme durumları gibi
        // check if expense exist
        // check if new user exist, check if new category exist

        var fromdb = await validate.RecordExistAsync<Expense>(x => x.ExpenseRequestId == request.ExpenseRequestId,
            cancellationToken);
        await validate.RecordExistAsync<User>(x => x.UserId == request.Model.UserId, cancellationToken);
        await validate.RecordExistAsync<ExpenseCategory>(x => x.CategoryId == request.Model.CategoryId,
            cancellationToken);

        fromdb.UserId = request.Model.UserId;
        fromdb.Amount = request.Model.Amount;
        fromdb.CategoryId = request.Model.CategoryId;
        fromdb.PaymentMethod = request.Model.PaymentMethod;
        fromdb.PaymentLocation = request.Model.PaymentLocation;
        fromdb.Documents = request.Model.Documents;
        fromdb.Status = (ExpenseRequestStatus)Enum.Parse(typeof(ExpenseRequestStatus), request.Model.Status, true);
        fromdb.Description = request.Model.Description;
        fromdb.PaymentStatus =
            (PaymentRequestStatus)Enum.Parse(typeof(PaymentRequestStatus), request.Model.PaymentStatus, true);
        fromdb.PaymentDescription = request.Model.PaymentDescription;

        await dbContext.SaveChangesAsync(cancellationToken);
        var mapped = mapper.Map<Expense, ExpenseResponse>(fromdb);
        return mapped;
    }

    public async Task<ExpenseResponse> Handle(DeleteExpenseCommand request, CancellationToken cancellationToken)
    {
        var fromdb = await validate.RecordExistAsync<Expense>(x => x.ExpenseRequestId == request.ExpenseRequestId,
            cancellationToken);

        dbContext.Remove(fromdb);
        await dbContext.SaveChangesAsync(cancellationToken);
        var mapped = mapper.Map<Expense, ExpenseResponse>(fromdb);
        return mapped;
    }

    public async Task<ExpenseResponse> Handle(ApproveExpenseCommand request, CancellationToken cancellationToken)
    {
        /*
         * check if expense exist
         * check if it is not approved and payment status not completed
         * do not check user exist since it cant be deleted if expense exist
         */

        var fromdb = await validate.RecordExistAsync<Expense>(x => x.ExpenseRequestId == request.ExpenseRequestId,
            cancellationToken);
        var awaiter = await validate.ExpenseCanBeApprovedAsync(fromdb, cancellationToken);
        
        fromdb.Status = ExpenseRequestStatus.Approved;
        fromdb.PaymentStatus = PaymentRequestStatus.OnProcess;
        fromdb.LastUpdateTime = DateTime.Now;
        
        await dbContext.SaveChangesAsync(cancellationToken);
        var mapped = mapper.Map<Expense, ExpenseResponse>(fromdb);
        
        double amount = fromdb.Amount;
        int fromUserId = fromdb.UserId;
        int toUserId = fromdb.CreatedBy;
        
        // Start Payment Service
        var jobId = BackgroundJob.Enqueue(() => payment.ProcessPayment(request.ExpenseRequestId, amount, fromUserId, toUserId));
        // BackgroundJob.ContinueJobWith(jobId, () => Console.WriteLine("Continuation!"));
        // // payment description and status will be updated by payment service also lastupdate time


        return mapped;
    }

    public async Task<ExpenseResponse> Handle(RejectExpenseCommand request, CancellationToken cancellationToken)
    {
        var fromdb = await validate.RecordExistAsync<Expense>(x => x.ExpenseRequestId == request.ExpenseRequestId,
            cancellationToken);
        fromdb.Status = ExpenseRequestStatus.Rejected;
        fromdb.PaymentDescription = request.PaymentDescription ?? fromdb.PaymentDescription;
        fromdb.PaymentStatus = PaymentRequestStatus.Declined;
        fromdb.LastUpdateTime = DateTime.Now;
        await dbContext.SaveChangesAsync(cancellationToken);
        var mapped = mapper.Map<Expense, ExpenseResponse>(fromdb);
        return mapped;
    }
}
/*
 *
 *        /* eger fromdb.PaymentRequestStatus Completed degil ve request.Model Completed ise jobı çalıştır.* /
   // Start Payment Service

   expense.LastUpdateTime = DateTime.Now;
   (decimal amount, string fromIban, string toIban) = (100, "TR123456789", "TR987654321");
   var jobId = BackgroundJob.Enqueue(() => payment.ProcessPayment(amount, fromIban, toIban));
   // First update payment instruction status to processing
   BackgroundJob.ContinueJobWith(jobId, () => Console.WriteLine("Continuation!"));
 *
 */