cd ExpenseApplication/

dotnet ef migrations add InitialCreate --project Infrastructure --startup-project Api
dotnet ef database update --project Infrastructure --startup-project Api



![erd.png](.github%2Fassets%2Ferd.png)


Documentation details https://documenter.getpostman.com/view/23348379/2s9YymFQ2G



<table>
  <tr>
    <th>Path</th>
    <td>
      <img src=".github/assets/POST.png" alt="POST" width="100"/>
    </td>
  </tr>
</table>
