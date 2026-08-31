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
  // Reads use Dapper; tracked writes use EF Core for audit and soft-delete interception.

  public async Task<Customer?> GetByIdAsync(Guid id, CancellationToken cancellationToken, bool tracked = false)
  {
    if (tracked)
    {
      return await context.Customers
          .FirstOrDefaultAsync(x => x.Id == id && x.Deleted == null, cancellationToken);
    }

    using IDbConnection db = connectionFactory.CreateConnection();
    return await db.QuerySingleOrDefaultAsync<Customer>(new CommandDefinition(
      "SELECT Id, Name, Email, Created, CreatedBy, Updated, UpdatedBy, Deleted, DeletedBy, RowVersion FROM Customers WHERE Id = @Id AND Deleted IS NULL",
      new { Id = id },
      cancellationToken: cancellationToken));
  }

  public async Task<IReadOnlyList<Customer>> GetAllAsync(CancellationToken cancellationToken)
  {
    var page = await GetPageAsync(PageRequest.Create(null, null), cancellationToken);
    return page.Items;
  }

  public async Task<PagedResult<Customer>> GetPageAsync(PageRequest page, CancellationToken cancellationToken)
  {
    using IDbConnection db = connectionFactory.CreateConnection();
    var result = await db.QueryAsync<Customer>(new CommandDefinition(
      """
      SELECT TOP (@Take) Id, Name, Email, Created, CreatedBy, Updated, UpdatedBy, Deleted, DeletedBy, RowVersion
      FROM Customers
      WHERE Deleted IS NULL AND (@After IS NULL OR Id > @After)
      ORDER BY Id
      """,
      new { Take = page.PageSize + PageRequest.LookaheadSize, page.After },
      cancellationToken: cancellationToken));
    var items = result.AsList();
    var hasMore = items.Count > page.PageSize;
    if (hasMore)
      items.RemoveAt(items.Count - 1);

    return new PagedResult<Customer>(items, hasMore ? items[^1].Id : null);
  }

  public async Task<bool> EmailExistsAsync(string email, CancellationToken cancellationToken)
  {
    using IDbConnection db = connectionFactory.CreateConnection();
    var count = await db.ExecuteScalarAsync<int>(new CommandDefinition(
      "SELECT COUNT(1) FROM Customers WHERE Email = @Email AND Deleted IS NULL",
      new { Email = email },
      cancellationToken: cancellationToken));
    return count > 0;
  }

  public async Task AddAsync(Customer customer, CancellationToken cancellationToken)
  {
    await context.Customers.AddAsync(customer, cancellationToken);
  }

  public void Remove(Customer customer)
  {
    context.Customers.Remove(customer);
  }

  public void SetOriginalRowVersion(Customer customer, byte[] rowVersion)
  {
    context.Entry(customer).Property(x => x.RowVersion).OriginalValue = rowVersion;
  }

  public async Task SaveChangesAsync(CancellationToken cancellationToken)
  {
    await context.SaveChangesAsync(cancellationToken);
  }
}
