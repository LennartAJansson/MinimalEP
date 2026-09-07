namespace MinimalEP.Features.Customer.UpdateCustomer;

using MinimalEP.Features.Core;
using MinimalEP.Infrastructure.Auth;

public class UpdateCustomerEndpoint
  : IEndpoint
{
  public IEndpointConventionBuilder MapEndpoint(IEndpointRouteBuilder builder)
  {
    return builder.MapPut(ApiRoutes.Customers.ById, async (
      Guid id,
      UpdateCustomerRequest request,
      IRequestHandler<UpdateCustomerRequest, Result<UpdateCustomerResponse>> handler,
      CancellationToken cancellationToken) =>
    {
      var result = await handler.HandleAsync(request with { Id = id }, cancellationToken);

      return result.ToHttpResult(ok => TypedResults.Ok(ok));
    }).RequireAuthorization(AuthorizationPolicies.AdminOrAbove);
  }
}
