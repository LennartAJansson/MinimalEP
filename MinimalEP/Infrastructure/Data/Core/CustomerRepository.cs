namespace MinimalEP.Infrastructure.Data.Core;

using System.Data;

using Dapper;

using Microsoft.EntityFrameworkCore;

using MinimalEP.Domain.Model;
using MinimalEP.Features.Core;
using MinimalEP.Infrastructure.Data.Context;

public class CustomerRepository(ApplicationDbContext context, IDbConnectionFactory connectionFactory)
  : ICustomerRepository
{
  // --- Läsoperationer via Dapper ---

  public async Task<Customer?> GetByIdAsync(Guid id, CancellationToken cancellationToken, bool tracked = false)
  {
    // tracked = true behövs av EF för att spåra ändringar vid Update/Delete
    if (tracked)
    {
      return await context.Customers
          .FirstOrDefaultAsync(x => x.Id == id && x.Deleted == null, cancellationToken);
    }

    using IDbConnection db = connectionFactory.CreateConnection();
    return await db.QuerySingleOrDefaultAsync<Customer>(
      "SELECT * FROM Customers WHERE Id = @Id AND Deleted IS NULL",
      new { Id = id });
  }

  public async Task<IReadOnlyList<Customer>> GetAllAsync(CancellationToken cancellationToken)
  {
    using IDbConnection db = connectionFactory.CreateConnection();
    var result = await db.QueryAsync<Customer>(
      "SELECT * FROM Customers WHERE Deleted IS NULL");
    return result.AsList();
  }

  public async Task<bool> EmailExistsAsync(string email, CancellationToken cancellationToken)
  {
    using IDbConnection db = connectionFactory.CreateConnection();
    var count = await db.ExecuteScalarAsync<int>(
      "SELECT COUNT(1) FROM Customers WHERE Email = @Email AND Deleted IS NULL",
      new { Email = email });
    return count > 0;
  }

  // --- Skrivoperationer via EF Core (interceptor hanterar audit + soft delete) ---

  public async Task AddAsync(Customer customer, CancellationToken cancellationToken)
  {
    await context.Customers.AddAsync(customer, cancellationToken);
  }

  public void Remove(Customer customer)
  {
    // AuditAndSoftDeleteInterceptor fångar upp detta och gör en soft delete!
    context.Customers.Remove(customer);
  }

  public async Task SaveChangesAsync(CancellationToken cancellationToken)
  {
    await context.SaveChangesAsync(cancellationToken);
  }
}
