namespace MinimalEP.Features.Customer.AddCustomer;

using System.Diagnostics;

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

        IResult httpResult = result switch
        {
          Result<AddCustomerResponse>.Ok ok       => TypedResults.CreatedAtRoute(ok.Value, ApiRouteNames.GetCustomer, new { version = ApiVersions.V1RouteValue, id = ok.Value.Id }),
          Result<AddCustomerResponse>.Conflict c  => TypedResults.Conflict(c.Message),
          _                                       => throw new UnreachableException()
        };

        return httpResult;
      }).RequireAuthorization(AuthorizationPolicies.AdminOrAbove);
    }
}
