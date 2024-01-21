// using System.Linq.Expressions;
// using Business.Entities;
// using Infrastructure.Data.DbContext;
// using Infrastructure.Exceptions;
// using Microsoft.EntityFrameworkCore;
//
// namespace Application.Validators;
//
//
// public interface IHandlerValidator
// {
//     // Task<User> ValidateUserIsExistAsync(int userId, CancellationToken cancellationToken);
//     // Task<ExpenseCategory> ValidateCategoryIsExistAsync(int categoryId, CancellationToken cancellationToken);
//     //
//     // Task<ExpenseCategory?> ValidateCategoryNameNotExistAsync(string modelCategoryName,
//     //     CancellationToken cancellationToken);
//     // Task<bool> ValidateNoActiveExpenseExistAsync(int id, Expression<Func<Expense, bool>> predicate, CancellationToken cancellationToken);
//     
//     // Task<T?> ValidateExistsAsync<T>(Expression<Func<T, bool>> query, CancellationToken cancellationToken) where T : class;
//     //
//     // Task<bool> ValidateNotExistsAsync<T>(Func<IQueryable<T>, IQueryable<T>> query, CancellationToken cancellationToken) where T : class;
//     
//     // Task<T?> ValidateExistsAsync<T>(Expression<Func<T, bool>> predicate, CancellationToken cancellationToken) where T : class;
//     //
//     // Task<bool> ValidateNotExistsAsync<T>(Expression<Func<T, bool>> predicate, CancellationToken cancellationToken) where T : class;
//     
//     Task<bool> ActiveExpenseNotExistAsync(int id, Expression<Func<Expense, bool>> predicate, CancellationToken cancellationToken);
//     Task<T> RecordExistAsync<T>(Expression<Func<T, bool>> predicate, CancellationToken cancellationToken) where T : class;
//     
//     Task<bool> RecordNotExistAsync<T>(Expression<Func<T, bool>> predicate, CancellationToken cancellationToken) where T : class;
//     
// }
//
//
// public class HandlerValidator : IHandlerValidator
// {
//     private readonly ExpenseDbContext dbContext;
//
//     public HandlerValidator(ExpenseDbContext dbContext)
//     {
//         this.dbContext = dbContext;
//     }
//
//     // public async Task<User> ValidateUserIsExistAsync(int userId, CancellationToken cancellationToken)
//     // {
//     //     var user = await dbContext.Users.FirstOrDefaultAsync(x => x.UserId == userId, cancellationToken);
//     //
//     //     if (user == null)
//     //     {
//     //         throw new HttpException("User not found", 404);
//     //     }
//     //
//     //     return user;
//     // }
//     //
//     // public async Task<ExpenseCategory> ValidateCategoryIsExistAsync(int categoryId, CancellationToken cancellationToken)
//     // {
//     //     var category = await dbContext.ExpenseCategories.FirstOrDefaultAsync(x => x.CategoryId == categoryId, cancellationToken);
//     //
//     //     if (category == null)
//     //     {
//     //         throw new HttpException("Category not found", 404);
//     //     }
//     //     
//     //     return category;
//     // }
//     //
//     // public async Task<ExpenseCategory?> ValidateCategoryNameNotExistAsync(string categoryName, CancellationToken cancellationToken)
//     // {
//     //     var category = await dbContext.ExpenseCategories.FirstOrDefaultAsync(x => x.CategoryName == categoryName.ToUpper(), cancellationToken);
//     //
//     //     if (category != null)
//     //     {
//     //         throw new HttpException("Category name already exist", 409);
//     //     }
//     //     
//     //     return category;
//     // }
//     
//     public async Task<bool> ActiveExpenseNotExistAsync(int id, Expression<Func<Expense, bool>> predicate, CancellationToken cancellationToken)
//     {
//         var entity = await dbContext.Expenses.Where(predicate).FirstOrDefaultAsync(cancellationToken);
//
//         if (entity != null)
//         {
//             throw new HttpException($"Active Expenses exist on this id: {id}", 405);
//         }
//
//         return true;
//     }
//     
//     public async Task<T> RecordExistAsync<T>(Expression<Func<T, bool>> predicate,
//         CancellationToken cancellationToken) where T : class
//     {
//         T? entity = await dbContext.Set<T>().FirstOrDefaultAsync<T>(predicate, cancellationToken);
//
//         if (entity == null)
//         {
//             throw new HttpException($"No record found in {typeof(T).Name}", 404);
//         }
//
//         return entity;
//     }
//     
//     public async Task<bool> RecordNotExistAsync<T>(Expression<Func<T, bool>> predicate,
//         CancellationToken cancellationToken) where T : class
//     {
//         T? entity = await dbContext.Set<T>().FirstOrDefaultAsync<T>(predicate, cancellationToken);
//         
//         if (entity != null)
//         {
//             throw new HttpException($"Existing record in {typeof(T).Name} table", 409);
//         }
//
//         return true;
//     }
//     
//     // public async Task<T?> ValidateExistsAsync<T>(Func<IQueryable<T>, IQueryable<T>> query, CancellationToken cancellationToken) where T : class
//     // {
//     //     // T? entity = await query(dbContext.Set<T>()).FirstOrDefaultAsync(cancellationToken);
//     //     T? entity = await query(dbContext.Set<T>()).FirstOrDefaultAsync(cancellationToken);
//     //
//     //     if (entity == null)
//     //     {
//     //         throw new HttpException($"{typeof(T)} record not found", 404);
//     //     }
//     //
//     //     return entity;
//     // }
//
//     // public async Task<bool> ValidateNotExistsAsync<T>(Func<IQueryable<T>, IQueryable<T>> query, CancellationToken cancellationToken) where T : class
//     // {
//     //     T? entity = await query(dbContext.Set<T>()).FirstOrDefaultAsync(cancellationToken);
//     //
//     //     if (entity != null)
//     //     {
//     //         throw new HttpException($"Existing record in {typeof(T)}", 409);
//     //     }
//     //
//     //     return true;
//     // }
//
//
//     
//     
//     
//     
// }
//