namespace MinimalEP.Features.Customer.UpdateCustomer;

using System.Diagnostics;

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

      IResult httpResult = result switch
      {
        Result<UpdateCustomerResponse>.Ok ok    => TypedResults.Ok(ok.Value),
        Result<UpdateCustomerResponse>.NotFound => TypedResults.NotFound(),
        Result<UpdateCustomerResponse>.Conflict c => TypedResults.Conflict(c.Message),
        _                                       => throw new UnreachableException()
      };

      return httpResult;
    }).RequireAuthorization(AuthorizationPolicies.AdminOrAbove);
  }
}
