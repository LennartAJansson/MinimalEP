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
  // Dapper mappar platta kolumner — navigationsegenskaperna Customer och Employee
  // populeras manuellt via multi-mapping. Employee.Address (owned type) is intentionally
  // not selected here — this query only needs the employee's name for display purposes.
  private const string BaseSelect = """
    SELECT
      w.Id, w.CustomerId, w.EmployeeId, w.Start, w.Stop, w.Comments,
      w.Created, w.CreatedBy, w.Updated, w.UpdatedBy, w.Deleted, w.DeletedBy,
      c.Id, c.Name, c.Email,
      c.Created, c.CreatedBy, c.Updated, c.UpdatedBy, c.Deleted, c.DeletedBy,
      e.Id, e.GivenName, e.Surname, e.Age, e.Position,
      e.Created, e.CreatedBy, e.Updated, e.UpdatedBy, e.Deleted, e.DeletedBy
    FROM Workloads w
    INNER JOIN Customers c ON c.Id = w.CustomerId
    INNER JOIN Employees e ON e.Id = w.EmployeeId
    WHERE w.Deleted IS NULL
    """;

  private static Workload Map(Workload w, Customer c, Employee e)
  {
    w.Customer = c;
    w.Employee = e;
    return w;
  }

  // --- Läsoperationer via Dapper ---

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
      $"{BaseSelect} AND w.Id = @Id",
      Map,
      new { Id = id },
      splitOn: "Id,Id");

    return result.SingleOrDefault();
  }

  public async Task<IReadOnlyList<Workload>> GetAllAsync(CancellationToken cancellationToken)
  {
    using IDbConnection db = connectionFactory.CreateConnection();
    var result = await db.QueryAsync<Workload, Customer, Employee, Workload>(
      BaseSelect, Map, splitOn: "Id,Id");
    return result.AsList();
  }

  public async Task<IReadOnlyList<Workload>> GetByCustomerAsync(Guid customerId, CancellationToken cancellationToken)
  {
    using IDbConnection db = connectionFactory.CreateConnection();
    var result = await db.QueryAsync<Workload, Customer, Employee, Workload>(
      $"{BaseSelect} AND w.CustomerId = @CustomerId",
      Map,
      new { CustomerId = customerId },
      splitOn: "Id,Id");
    return result.AsList();
  }

  public async Task<IReadOnlyList<Workload>> GetByEmployeeAsync(Guid employeeId, CancellationToken cancellationToken)
  {
    using IDbConnection db = connectionFactory.CreateConnection();
    var result = await db.QueryAsync<Workload, Customer, Employee, Workload>(
      $"{BaseSelect} AND w.EmployeeId = @EmployeeId",
      Map,
      new { EmployeeId = employeeId },
      splitOn: "Id,Id");
    return result.AsList();
  }

  // --- Skrivoperationer via EF Core ---

  public async Task AddAsync(Workload workload, CancellationToken cancellationToken)
  {
    await context.Workloads.AddAsync(workload, cancellationToken);
  }

  public void Remove(Workload workload)
  {
    context.Workloads.Remove(workload);
  }

  public async Task SaveChangesAsync(CancellationToken cancellationToken)
  {
    await context.SaveChangesAsync(cancellationToken);
  }
}
