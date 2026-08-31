namespace MinimalEP.Features.Customer.DeleteCustomer;

using System.Diagnostics;

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

      IResult httpResult = result switch
      {
        Result<Unit>.Ok       => TypedResults.NoContent(),
        Result<Unit>.NotFound => TypedResults.NotFound(),
        _                     => throw new UnreachableException()
      };

      return httpResult;
    }).RequireAuthorization(AuthorizationPolicies.AdminOrAbove);
  }
}
