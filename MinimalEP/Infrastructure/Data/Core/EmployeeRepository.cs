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
      Created, CreatedBy, Updated, UpdatedBy, Deleted, DeletedBy,
      Address_Street AS Street, Address_PostalCode AS PostalCode, Address_City AS City
    FROM Employees
    WHERE Deleted IS NULL
    """;

  private static Employee Map(Employee e, Address a)
  {
    e.Address = a;
    return e;
  }

  // --- Läsoperationer via Dapper ---

  public async Task<Employee?> GetByIdAsync(Guid id, CancellationToken cancellationToken, bool tracked = false)
  {
    if (tracked)
    {
      return await context.Employees
          .FirstOrDefaultAsync(x => x.Id == id && x.Deleted == null, cancellationToken);
    }

    using IDbConnection db = connectionFactory.CreateConnection();
    var result = await db.QueryAsync<Employee, Address, Employee>(
      $"{BaseSelect} AND Id = @Id",
      Map,
      new { Id = id },
      splitOn: "Street");

    return result.SingleOrDefault();
  }

  public async Task<IReadOnlyList<Employee>> GetAllAsync(CancellationToken cancellationToken)
  {
    using IDbConnection db = connectionFactory.CreateConnection();
    var result = await db.QueryAsync<Employee, Address, Employee>(
      BaseSelect, Map, splitOn: "Street");
    return result.AsList();
  }

  // --- Skrivoperationer via EF Core ---

  public async Task AddAsync(Employee employee, CancellationToken cancellationToken)
  {
    await context.Employees.AddAsync(employee, cancellationToken);
  }

  public void Remove(Employee employee)
  {
    context.Employees.Remove(employee);
  }

  public async Task SaveChangesAsync(CancellationToken cancellationToken)
  {
    await context.SaveChangesAsync(cancellationToken);
  }
}
