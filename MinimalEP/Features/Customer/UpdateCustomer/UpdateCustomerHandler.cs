namespace MinimalEP.Features.Customer.UpdateCustomer;

using MinimalEP.Features.Core;

public class UpdateCustomerHandler(ICustomerRepository repository)
  : IRequestHandler<UpdateCustomerRequest, Result<UpdateCustomerResponse>>
{
  public async Task<Result<UpdateCustomerResponse>> HandleAsync(UpdateCustomerRequest request, CancellationToken cancellationToken)
  {
    var customer = await repository.GetByIdAsync(request.Id, cancellationToken, tracked: true);

    if (customer is null)
      return new Result<UpdateCustomerResponse>.NotFound();

    request.ApplyTo(customer);
    await repository.SaveChangesAsync(cancellationToken);

    return new Result<UpdateCustomerResponse>.Ok(customer.ToResponse());
  }
}
