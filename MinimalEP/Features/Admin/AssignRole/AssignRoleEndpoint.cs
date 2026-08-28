namespace MinimalEP.Features.Admin.AssignRole;

using System.Diagnostics;

using MinimalEP.Features.Core;

public class AssignRoleEndpoint : IEndpoint
{
  public IEndpointConventionBuilder MapEndpoint(IEndpointRouteBuilder builder)
  {
    return builder.MapPut("/admin/users/{userId}/role", async (
      Guid userId,
      AssignRoleRequest request,
      IRequestHandler<AssignRoleRequest, Result<AssignRoleResponse>> handler,
      CancellationToken cancellationToken) =>
    {
      var result = await handler.HandleAsync(request with { UserId = userId }, cancellationToken);

      IResult httpResult = result switch
      {
        Result<AssignRoleResponse>.Ok ok       => TypedResults.Ok(ok.Value),
        Result<AssignRoleResponse>.NotFound    => TypedResults.NotFound(),
        Result<AssignRoleResponse>.Conflict c  => TypedResults.Conflict(c.Message),
        _                                      => throw new UnreachableException()
      };

      return httpResult;
    }).RequireAuthorization("AdminOrAbove");
  }
}
