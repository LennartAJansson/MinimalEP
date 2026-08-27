namespace MinimalEP.Features.Customer.AddCustomer;
using MinimalEP.Domain.Model;
using MinimalEP.Features.Core;

public class AddCustomerHandler(ICustomerRepository repository)
  : IRequestHandler<AddCustomerRequest, Result<AddCustomerResponse>>
{
  public async Task<Result<AddCustomerResponse>> HandleAsync(AddCustomerRequest request, CancellationToken cancellationToken)
  {
    if (await repository.EmailExistsAsync(request.Email, cancellationToken))
      return new Result<AddCustomerResponse>.Conflict($"A customer with email '{request.Email}' already exists.");

    Customer customer = request.ToEntity(Guid.CreateVersion7());

    await repository.AddAsync(customer, cancellationToken);
    await repository.SaveChangesAsync(cancellationToken);

    return new Result<AddCustomerResponse>.Ok(customer.ToResponse());
  }
}