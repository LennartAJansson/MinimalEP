namespace MinimalEP.Features.Customer.GetCustomer;

using MinimalEP.Features.Core;
using MinimalEP.Infrastructure.Auth;

public class GetCustomerEndpoint
  : IEndpoint
{
  public IEndpointConventionBuilder MapEndpoint(IEndpointRouteBuilder builder)
  {
    return builder.MapGet(ApiRoutes.Customers.ById, async (
      Guid id,
      IRequestHandler<GetCustomerRequest, Result<GetCustomerResponse>> handler,
      CancellationToken cancellationToken) =>
    {
      var result = await handler.HandleAsync(new GetCustomerRequest(id), cancellationToken);

      return result.ToHttpResult(ok => TypedResults.Ok(ok));
    }).WithName(ApiRouteNames.GetCustomer).RequireAuthorization(AuthorizationPolicies.AdminOrAbove);
  }
}
