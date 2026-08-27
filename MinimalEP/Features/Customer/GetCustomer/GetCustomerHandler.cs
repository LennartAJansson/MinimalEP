namespace MinimalEP.Features.Customer.GetCustomer;

using MinimalEP.Features.Core;

public class GetCustomerHandler(ICustomerRepository repository)
  : IRequestHandler<GetCustomerRequest, Result<GetCustomerResponse>>
{
  public async Task<Result<GetCustomerResponse>> HandleAsync(GetCustomerRequest request, CancellationToken cancellationToken)
  {
    var customer = await repository.GetByIdAsync(request.Id, cancellationToken);

    return customer is null
      ? new Result<GetCustomerResponse>.NotFound()
      : new Result<GetCustomerResponse>.Ok(customer.ToResponse());
  }
}
