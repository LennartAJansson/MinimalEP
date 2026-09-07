namespace MinimalEP.Features.Customer.DeleteCustomer;

using MinimalEP.Features.Core;
using MinimalEP.Infrastructure.Auth;

public class DeleteCustomerEndpoint
  : IEndpoint
{
  public IEndpointConventionBuilder MapEndpoint(IEndpointRouteBuilder builder)
  {
    return builder.MapDelete(ApiRoutes.Customers.ById, async (
      Guid id,
      IRequestHandler<DeleteCustomerRequest, Result<Unit>> handler,
      CancellationToken cancellationToken) =>
    {
      var result = await handler.HandleAsync(new DeleteCustomerRequest(id), cancellationToken);

      return result.ToHttpResult(_ => TypedResults.NoContent());
    }).RequireAuthorization(AuthorizationPolicies.AdminOrAbove);
  }
}
