namespace MinimalEP.Infrastructure.Data.Core;

using System.Data;

using Dapper;

using Microsoft.EntityFrameworkCore;

using MinimalEP.Domain.Model;
using MinimalEP.Features.Core;
using MinimalEP.Infrastructure.Data.Context;

public class EmployeeRepository(ApplicationDbContext context, IDbConnectionFactory connectionFactory)
  : IEmployeeRepository
{
  // Dapper can't populate an owned-type navigation property (Address) via a single flat
  // mapping, so the query is split into Employee + Address parts and stitched together
  // manually, the same multi-mapping technique used for Workload's Customer/Employee.
  private const string BaseSelect = """
    SELECT
      Id, Email, GivenName, Surname, Age, Position, PhoneNumber,
      Created, CreatedBy, Updated, UpdatedBy, Deleted, DeletedBy, RowVersion,
      Address_Street AS Street, Address_PostalCode AS PostalCode, Address_City AS City
    FROM Employees
    WHERE Deleted IS NULL
    """;

  private static Employee Map(Employee e, Address a)
  {
    e.Address = a;
    return e;
  }

  // Reads use Dapper; tracked writes use EF Core.

  public async Task<Employee?> GetByIdAsync(Guid id, CancellationToken cancellationToken, bool tracked = false)
  {
    if (tracked)
    {
      return await context.Employees
          .FirstOrDefaultAsync(x => x.Id == id && x.Deleted == null, cancellationToken);
    }

    using IDbConnection db = connectionFactory.CreateConnection();
    var result = await db.QueryAsync<Employee, Address, Employee>(
      new CommandDefinition(
        $"{BaseSelect} AND Id = @Id",
        new { Id = id },
        cancellationToken: cancellationToken),
      Map,
      splitOn: "Street");

    return result.SingleOrDefault();
  }

  public async Task<IReadOnlyList<Employee>> GetAllAsync(CancellationToken cancellationToken)
  {
    var page = await GetPageAsync(PageRequest.Create(null, null), cancellationToken);
    return page.Items;
  }

  public async Task<PagedResult<Employee>> GetPageAsync(PageRequest page, CancellationToken cancellationToken)
  {
    using IDbConnection db = connectionFactory.CreateConnection();
    var result = await db.QueryAsync<Employee, Address, Employee>(
      new CommandDefinition(
        """
        SELECT TOP (@Take)
          Id, Email, GivenName, Surname, Age, Position, PhoneNumber,
          Created, CreatedBy, Updated, UpdatedBy, Deleted, DeletedBy, RowVersion,
          Address_Street AS Street, Address_PostalCode AS PostalCode, Address_City AS City
        FROM Employees
        WHERE Deleted IS NULL AND (@After IS NULL OR Id > @After)
        ORDER BY Id
        """,
        new { Take = page.PageSize + PageRequest.LookaheadSize, page.After },
        cancellationToken: cancellationToken),
      Map,
      splitOn: "Street");
    var items = result.AsList();
    var hasMore = items.Count > page.PageSize;
    if (hasMore)
      items.RemoveAt(items.Count - 1);

    return new PagedResult<Employee>(items, hasMore ? items[^1].Id : null);
  }

  public async Task AddAsync(Employee employee, CancellationToken cancellationToken)
  {
    await context.Employees.AddAsync(employee, cancellationToken);
  }

  public void Remove(Employee employee)
  {
    context.Employees.Remove(employee);
  }

  public void SetOriginalRowVersion(Employee employee, byte[] rowVersion)
  {
    context.Entry(employee).Property(x => x.RowVersion).OriginalValue = rowVersion;
  }

  public async Task SaveChangesAsync(CancellationToken cancellationToken)
  {
    await context.SaveChangesAsync(cancellationToken);
  }
}
