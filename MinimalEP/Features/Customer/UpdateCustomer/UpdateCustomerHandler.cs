namespace MinimalEP.Features.Customer.UpdateCustomer;

using Microsoft.EntityFrameworkCore;

using MinimalEP.Features.Core;

public class UpdateCustomerHandler(ICustomerRepository repository)
  : IRequestHandler<UpdateCustomerRequest, Result<UpdateCustomerResponse>>
{
  public async Task<Result<UpdateCustomerResponse>> HandleAsync(UpdateCustomerRequest request, CancellationToken cancellationToken)
  {
    var customer = await repository.GetByIdAsync(request.Id, cancellationToken, tracked: true);

    if (customer is null)
      return new Result<UpdateCustomerResponse>.NotFound();

    repository.SetOriginalRowVersion(customer, request.RowVersion);
    request.ApplyTo(customer);
    try
    {
      await repository.SaveChangesAsync(cancellationToken);
    }
    catch (DbUpdateConcurrencyException)
    {
      return new Result<UpdateCustomerResponse>.Conflict("The customer was changed by another request. Reload it and try again.");
    }

    return new Result<UpdateCustomerResponse>.Ok(customer.ToResponse());
  }
}
