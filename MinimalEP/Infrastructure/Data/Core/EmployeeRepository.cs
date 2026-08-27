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
  // --- Läsoperationer via Dapper ---

  public async Task<Employee?> GetByIdAsync(Guid id, CancellationToken cancellationToken, bool tracked = false)
  {
    if (tracked)
    {
      return await context.Employees
          .FirstOrDefaultAsync(x => x.Id == id && x.Deleted == null, cancellationToken);
    }

    using IDbConnection db = connectionFactory.CreateConnection();
    return await db.QuerySingleOrDefaultAsync<Employee>(
      "SELECT * FROM Employees WHERE Id = @Id AND Deleted IS NULL",
      new { Id = id });
  }

  public async Task<IReadOnlyList<Employee>> GetAllAsync(CancellationToken cancellationToken)
  {
    using IDbConnection db = connectionFactory.CreateConnection();
    var result = await db.QueryAsync<Employee>(
      "SELECT * FROM Employees WHERE Deleted IS NULL");
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
