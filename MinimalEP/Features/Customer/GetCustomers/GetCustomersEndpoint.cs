namespace MinimalEP.Features.Customer.GetCustomers;

using MinimalEP.Features.Core;
using MinimalEP.Infrastructure.Auth;

public class GetCustomersEndpoint
  : IEndpoint
{
  public IEndpointConventionBuilder MapEndpoint(IEndpointRouteBuilder builder)
  {
    return builder.MapGet(ApiRoutes.Customers.Collection, async (
      int? pageSize,
      Guid? after,
      IRequestHandler<GetCustomersRequest, Result<GetCustomersResponse>> handler,
      CancellationToken cancellationToken) =>
    {
      var result = await handler.HandleAsync(new GetCustomersRequest(pageSize, after), cancellationToken);

      return result.ToHttpResult(ok => TypedResults.Ok(ok));
    }).RequireAuthorization(AuthorizationPolicies.AdminOrAbove);
  }
}
