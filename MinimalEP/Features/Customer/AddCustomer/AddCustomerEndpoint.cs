namespace MinimalEP.Features.Customer.AddCustomer;

using MinimalEP.Features.Core;
using MinimalEP.Infrastructure.Auth;

public class AddCustomerEndpoint 
  : IEndpoint
{
    public IEndpointConventionBuilder MapEndpoint(IEndpointRouteBuilder builder)
    {
      return builder.MapPost(ApiRoutes.Customers.Collection, async (
        AddCustomerRequest request,
        IRequestHandler<AddCustomerRequest, Result<AddCustomerResponse>> handler,
        CancellationToken cancellationToken) =>
      {
        var result = await handler.HandleAsync(request, cancellationToken);

        return result.ToHttpResult(ok => TypedResults.CreatedAtRoute(ok, ApiRouteNames.GetCustomer, new { version = ApiVersions.V1RouteValue, id = ok.Id }));
      }).RequireAuthorization(AuthorizationPolicies.AdminOrAbove);
    }
}
