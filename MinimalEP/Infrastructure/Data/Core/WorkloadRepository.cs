namespace MinimalEP.Infrastructure.Data.Core;

using System.Data;

using Dapper;

using Microsoft.EntityFrameworkCore;

using MinimalEP.Domain.Model;
using MinimalEP.Features.Core;
using MinimalEP.Infrastructure.Data.Context;

public class WorkloadRepository(ApplicationDbContext context, IDbConnectionFactory connectionFactory)
  : IWorkloadRepository
{
  // Dapper maps flat columns, so Customer and Employee are populated through
  // multi-mapping. Employee.Address (owned type) is intentionally
  // not selected here — this query only needs the employee's name for display purposes.
  private const string BaseSelect = """
    SELECT
      w.Id, w.CustomerId, w.EmployeeId, w.Start, w.Stop, w.Comments,
      w.Created, w.CreatedBy, w.Updated, w.UpdatedBy, w.Deleted, w.DeletedBy, w.RowVersion,
      c.Id, c.Name, c.Email,
      c.Created, c.CreatedBy, c.Updated, c.UpdatedBy, c.Deleted, c.DeletedBy,
      e.Id, e.GivenName, e.Surname, e.Age, e.Position,
      e.Created, e.CreatedBy, e.Updated, e.UpdatedBy, e.Deleted, e.DeletedBy
    FROM Workloads w
    INNER JOIN Customers c ON c.Id = w.CustomerId
    INNER JOIN Employees e ON e.Id = w.EmployeeId
    WHERE w.Deleted IS NULL AND c.Deleted IS NULL AND e.Deleted IS NULL
    """;

  private static Workload Map(Workload w, Customer c, Employee e)
  {
    w.Customer = c;
    w.Employee = e;
    return w;
  }

  // Reads use Dapper; tracked writes use EF Core.

  public async Task<Workload?> GetByIdAsync(Guid id, CancellationToken cancellationToken, bool tracked = false)
  {
    if (tracked)
    {
      return await context.Workloads
          .Include(x => x.Customer)
          .Include(x => x.Employee)
          .FirstOrDefaultAsync(x => x.Id == id && x.Deleted == null, cancellationToken);
    }

    using IDbConnection db = connectionFactory.CreateConnection();
    var result = await db.QueryAsync<Workload, Customer, Employee, Workload>(
      new CommandDefinition(
        $"{BaseSelect} AND w.Id = @Id",
        new { Id = id },
        cancellationToken: cancellationToken),
      Map,
      splitOn: "Id,Id");

    return result.SingleOrDefault();
  }

  public async Task<IReadOnlyList<Workload>> GetAllAsync(CancellationToken cancellationToken)
  {
    var page = await GetPageAsync(PageRequest.Create(null, null), cancellationToken);
    return page.Items;
  }

  public Task<PagedResult<Workload>> GetPageAsync(PageRequest page, CancellationToken cancellationToken)
  {
    return QueryPageAsync(null, null, page, cancellationToken);
  }

  public async Task<IReadOnlyList<Workload>> GetByCustomerAsync(Guid customerId, CancellationToken cancellationToken)
  {
    var page = await GetByCustomerPageAsync(customerId, PageRequest.Create(null, null), cancellationToken);
    return page.Items;
  }

  public Task<PagedResult<Workload>> GetByCustomerPageAsync(Guid customerId, PageRequest page, CancellationToken cancellationToken)
  {
    return QueryPageAsync("w.CustomerId", customerId, page, cancellationToken);
  }

  public async Task<IReadOnlyList<Workload>> GetByEmployeeAsync(Guid employeeId, CancellationToken cancellationToken)
  {
    var page = await GetByEmployeePageAsync(employeeId, PageRequest.Create(null, null), cancellationToken);
    return page.Items;
  }

  public Task<PagedResult<Workload>> GetByEmployeePageAsync(Guid employeeId, PageRequest page, CancellationToken cancellationToken)
  {
    return QueryPageAsync("w.EmployeeId", employeeId, page, cancellationToken);
  }

  public async Task<bool> HasOpenWorkloadAsync(Guid employeeId, CancellationToken cancellationToken)
  {
    using IDbConnection db = connectionFactory.CreateConnection();
    return await db.ExecuteScalarAsync<bool>(new CommandDefinition(
      "SELECT CAST(CASE WHEN EXISTS (SELECT 1 FROM Workloads WHERE EmployeeId = @EmployeeId AND Stop IS NULL AND Deleted IS NULL) THEN 1 ELSE 0 END AS bit)",
      new { EmployeeId = employeeId },
      cancellationToken: cancellationToken));
  }

  private async Task<PagedResult<Workload>> QueryPageAsync(
    string? filterColumn,
    Guid? filterId,
    PageRequest page,
    CancellationToken cancellationToken)
  {
    using IDbConnection db = connectionFactory.CreateConnection();
    var filter = filterColumn is null ? string.Empty : $" AND {filterColumn} = @FilterId";
    var result = await db.QueryAsync<Workload, Customer, Employee, Workload>(
      new CommandDefinition(
        $"{BaseSelect}{filter} AND (@After IS NULL OR w.Id < @After) ORDER BY w.Id DESC OFFSET 0 ROWS FETCH NEXT @Take ROWS ONLY",
        new { FilterId = filterId, page.After, Take = page.PageSize + PageRequest.LookaheadSize },
        cancellationToken: cancellationToken),
      Map,
      splitOn: "Id,Id");
    var items = result.AsList();
    var hasMore = items.Count > page.PageSize;
    if (hasMore)
      items.RemoveAt(items.Count - 1);

    return new PagedResult<Workload>(items, hasMore ? items[^1].Id : null);
  }

  public async Task AddAsync(Workload workload, CancellationToken cancellationToken)
  {
    await context.Workloads.AddAsync(workload, cancellationToken);
  }

  public void Remove(Workload workload)
  {
    context.Workloads.Remove(workload);
  }

  public void SetOriginalRowVersion(Workload workload, byte[] rowVersion)
  {
    context.Entry(workload).Property(x => x.RowVersion).OriginalValue = rowVersion;
  }

  public async Task SaveChangesAsync(CancellationToken cancellationToken)
  {
    await context.SaveChangesAsync(cancellationToken);
  }
}
