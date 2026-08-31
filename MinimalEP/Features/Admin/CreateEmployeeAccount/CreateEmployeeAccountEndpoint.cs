namespace MinimalEP.Features.Admin.CreateEmployeeAccount;

using System.Diagnostics;

using MinimalEP.Features.Core;
using MinimalEP.Infrastructure.Auth;

public class CreateEmployeeAccountEndpoint : IEndpoint
{
  public IEndpointConventionBuilder MapEndpoint(IEndpointRouteBuilder builder)
  {
    return builder.MapPost(ApiRoutes.Admin.Employees, async (
      CreateEmployeeAccountRequest request,
      IRequestHandler<CreateEmployeeAccountRequest, Result<CreateEmployeeAccountResponse>> handler,
      CancellationToken cancellationToken) =>
    {
      var result = await handler.HandleAsync(request, cancellationToken);

      IResult httpResult = result switch
      {
        Result<CreateEmployeeAccountResponse>.Ok ok      => TypedResults.CreatedAtRoute(ok.Value, ApiRouteNames.GetEmployee, new { version = ApiVersions.V1RouteValue, id = ok.Value.UserId }),
        Result<CreateEmployeeAccountResponse>.Conflict c => TypedResults.Conflict(c.Message),
        _                                                => throw new UnreachableException()
      };

      return httpResult;
    }).RequireAuthorization(AuthorizationPolicies.AdminOrAbove);
  }
}
