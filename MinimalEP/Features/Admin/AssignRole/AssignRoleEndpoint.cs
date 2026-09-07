namespace MinimalEP.Features.Admin.AssignRole;

using MinimalEP.Features.Core;
using MinimalEP.Infrastructure.Auth;

public class AssignRoleEndpoint : IEndpoint
{
  public IEndpointConventionBuilder MapEndpoint(IEndpointRouteBuilder builder)
  {
    return builder.MapPut(ApiRoutes.Admin.UserRole, async (
      Guid userId,
      AssignRoleRequest request,
      IRequestHandler<AssignRoleRequest, Result<AssignRoleResponse>> handler,
      CancellationToken cancellationToken) =>
    {
      var result = await handler.HandleAsync(request with { UserId = userId }, cancellationToken);

      return result.ToHttpResult(ok => TypedResults.Ok(ok));
    }).RequireAuthorization(AuthorizationPolicies.AdminOrAbove);
  }
}
