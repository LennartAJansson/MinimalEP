namespace MinimalEP.Tests.Infrastructure;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

using MinimalEP.Domain.Model;
using MinimalEP.Features.Core;
using MinimalEP.Infrastructure.Data.Context;

[Collection(IntegrationCollection.Name)]
public sealed class DatabaseIntegrationTests(MinimalEpApplicationFactory factory)
{
  [Fact]
  public async Task Customer_repository_honors_soft_delete()
  {
    var id = Guid.CreateVersion7();
    await using var scope = factory.Services.CreateAsyncScope();
    var repository = scope.ServiceProvider.GetRequiredService<ICustomerRepository>();
    await repository.AddAsync(new Customer
    {
      Id = id,
      Name = "Integration customer",
      Email = $"customer-{id:N}@example.test"
    }, CancellationToken.None);
    await repository.SaveChangesAsync(CancellationToken.None);

    var tracked = await repository.GetByIdAsync(id, CancellationToken.None, tracked: true);
    Assert.NotNull(tracked);
    repository.Remove(tracked);
    await repository.SaveChangesAsync(CancellationToken.None);

    var deleted = await repository.GetByIdAsync(id, CancellationToken.None);
    Assert.Null(deleted);
  }

  [Fact]
  public async Task Customer_repository_pages_with_a_bounded_non_overlapping_cursor()
  {
    await using var scope = factory.Services.CreateAsyncScope();
    var repository = scope.ServiceProvider.GetRequiredService<ICustomerRepository>();
    for (var i = 0; i < 120; i++)
    {
      var id = Guid.CreateVersion7();
      await repository.AddAsync(new Customer
      {
        Id = id,
        Name = $"Paged customer {i}",
        Email = $"paged-customer-{id:N}@example.test"
      }, CancellationToken.None);
    }
    await repository.SaveChangesAsync(CancellationToken.None);

    var first = await repository.GetPageAsync(PageRequest.Create(null, 100), CancellationToken.None);
    var second = await repository.GetPageAsync(PageRequest.Create(first.NextCursor, 100), CancellationToken.None);

    Assert.Equal(100, first.Items.Count);
    Assert.NotNull(first.NextCursor);
    Assert.NotEmpty(second.Items);
    Assert.Empty(first.Items.Select(x => x.Id).Intersect(second.Items.Select(x => x.Id)));
  }

  [Fact]
  public async Task Dapper_read_honors_cancellation()
  {
    await using var scope = factory.Services.CreateAsyncScope();
    var repository = scope.ServiceProvider.GetRequiredService<ICustomerRepository>();
    using var cancellation = new CancellationTokenSource();
    cancellation.Cancel();

    await Assert.ThrowsAnyAsync<OperationCanceledException>(() =>
      repository.GetPageAsync(PageRequest.Create(null, null), cancellation.Token));
  }

  [Fact]
  public async Task Database_rejects_two_open_workloads_for_the_same_employee()
  {
    var customer = CreateCustomer();
    var employee = CreateEmployee();
    await using var scope = factory.Services.CreateAsyncScope();
    var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    context.AddRange(customer, employee);
    await context.SaveChangesAsync(CancellationToken.None);
    context.Workloads.Add(CreateOpenWorkload(customer.Id, employee.Id));
    await context.SaveChangesAsync(CancellationToken.None);
    context.ChangeTracker.Clear();
    context.Workloads.Add(CreateOpenWorkload(customer.Id, employee.Id));

    await Assert.ThrowsAsync<DbUpdateException>(() =>
      context.SaveChangesAsync(CancellationToken.None));
  }

  [Fact]
  public async Task Refresh_token_rowversion_rejects_concurrent_rotation()
  {
    var tokenId = Guid.CreateVersion7();
    await using (var setupScope = factory.Services.CreateAsyncScope())
    {
      var setupContext = setupScope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
      setupContext.RefreshTokens.Add(new RefreshToken
      {
        Id = tokenId,
        UserId = Guid.CreateVersion7(),
        FamilyId = Guid.CreateVersion7(),
        TokenHash = Convert.ToBase64String(Guid.NewGuid().ToByteArray()),
        ExpiresAt = DateTimeOffset.UtcNow.AddDays(1)
      });
      await setupContext.SaveChangesAsync(CancellationToken.None);
    }

    await using var firstScope = factory.Services.CreateAsyncScope();
    await using var secondScope = factory.Services.CreateAsyncScope();
    var firstContext = firstScope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    var secondContext = secondScope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    var first = await firstContext.RefreshTokens.SingleAsync(x => x.Id == tokenId, CancellationToken.None);
    var second = await secondContext.RefreshTokens.SingleAsync(x => x.Id == tokenId, CancellationToken.None);
    first.RevokedAt = DateTimeOffset.UtcNow;
    second.RevokedAt = DateTimeOffset.UtcNow;
    await firstContext.SaveChangesAsync(CancellationToken.None);

    await Assert.ThrowsAsync<DbUpdateConcurrencyException>(() =>
      secondContext.SaveChangesAsync(CancellationToken.None));
  }

  [Fact]
  public async Task Customer_rowversion_rejects_concurrent_updates()
  {
    var customer = CreateCustomer();
    await using (var setupScope = factory.Services.CreateAsyncScope())
    {
      var setupContext = setupScope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
      setupContext.Customers.Add(customer);
      await setupContext.SaveChangesAsync(CancellationToken.None);
    }

    await using var firstScope = factory.Services.CreateAsyncScope();
    await using var secondScope = factory.Services.CreateAsyncScope();
    var firstContext = firstScope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    var secondContext = secondScope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    var first = await firstContext.Customers.SingleAsync(x => x.Id == customer.Id, CancellationToken.None);
    var second = await secondContext.Customers.SingleAsync(x => x.Id == customer.Id, CancellationToken.None);
    first.Name = "First update";
    second.Name = "Stale update";
    await firstContext.SaveChangesAsync(CancellationToken.None);

    await Assert.ThrowsAsync<DbUpdateConcurrencyException>(() =>
      secondContext.SaveChangesAsync(CancellationToken.None));
  }

  private static Customer CreateCustomer()
  {
    var id = Guid.CreateVersion7();
    return new Customer { Id = id, Name = "Customer", Email = $"customer-{id:N}@example.test" };
  }

  private static Employee CreateEmployee()
  {
    var id = Guid.CreateVersion7();
    return new Employee
    {
      Id = id,
      Email = $"employee-{id:N}@example.test",
      GivenName = "Test",
      Surname = "Employee",
      Age = 30,
      Position = "Tester",
      PhoneNumber = "000",
      Address = new Address { Street = "Street", PostalCode = "00000", City = "City" }
    };
  }

  private static Workload CreateOpenWorkload(Guid customerId, Guid employeeId) => new()
  {
    CustomerId = customerId,
    EmployeeId = employeeId,
    Start = DateTimeOffset.UtcNow
  };
}
