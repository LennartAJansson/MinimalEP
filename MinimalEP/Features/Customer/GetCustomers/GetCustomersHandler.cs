namespace MinimalEP.Features.Customer.GetCustomers;

using MinimalEP.Features.Core;

public class GetCustomersHandler(ICustomerRepository repository)
  : IRequestHandler<GetCustomersRequest, Result<GetCustomersResponse>>
{
  public async Task<Result<GetCustomersResponse>> HandleAsync(GetCustomersRequest request, CancellationToken cancellationToken)
  {
    var customers = await repository.GetPageAsync(PageRequest.Create(request.After, request.PageSize), cancellationToken);
    return new Result<GetCustomersResponse>.Ok(customers.ToResponse());
  }
}
