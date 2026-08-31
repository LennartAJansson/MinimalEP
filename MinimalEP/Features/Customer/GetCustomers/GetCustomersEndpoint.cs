namespace MinimalEP.Features.Customer.GetCustomers;

using System.Diagnostics;

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

      IResult httpResult = result switch
      {
        Result<GetCustomersResponse>.Ok ok => TypedResults.Ok(ok.Value),
        _ => throw new UnreachableException()
      };
      return httpResult;
    }).RequireAuthorization(AuthorizationPolicies.AdminOrAbove);
  }
}
