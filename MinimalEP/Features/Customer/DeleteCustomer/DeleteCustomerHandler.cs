namespace MinimalEP.Features.Customer.DeleteCustomer;

using MinimalEP.Features.Core;

public class DeleteCustomerHandler(ICustomerRepository repository)
  : IRequestHandler<DeleteCustomerRequest, Result<Unit>>
{
  public async Task<Result<Unit>> HandleAsync(DeleteCustomerRequest request, CancellationToken cancellationToken)
  {
    var customer = await repository.GetByIdAsync(request.Id, cancellationToken, tracked: true);

    if (customer is null)
      return new Result<Unit>.NotFound();

    repository.Remove(customer);
    await repository.SaveChangesAsync(cancellationToken);

    return new Result<Unit>.Ok(Unit.Value);
  }
}
