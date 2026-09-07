namespace MinimalEP.Features.Admin.CreateEmployeeAccount;

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

      return result.ToHttpResult(ok => TypedResults.CreatedAtRoute(ok, ApiRouteNames.GetEmployee, new { version = ApiVersions.V1RouteValue, id = ok.UserId }));
    }).RequireAuthorization(AuthorizationPolicies.AdminOrAbove);
  }
}
