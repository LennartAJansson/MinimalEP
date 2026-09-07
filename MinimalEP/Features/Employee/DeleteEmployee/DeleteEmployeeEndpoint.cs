namespace MinimalEP.Features.Employee.DeleteEmployee;

using MinimalEP.Features.Core;
using MinimalEP.Infrastructure.Auth;

public class DeleteEmployeeEndpoint : IEndpoint
{
  public IEndpointConventionBuilder MapEndpoint(IEndpointRouteBuilder builder)
  {
    return builder.MapDelete(ApiRoutes.Employees.ById, async (
      Guid id,
      IRequestHandler<DeleteEmployeeRequest, Result<Unit>> handler,
      CancellationToken cancellationToken) =>
    {
      var result = await handler.HandleAsync(new DeleteEmployeeRequest(id), cancellationToken);

      return result.ToHttpResult(_ => TypedResults.NoContent());
    }).RequireAuthorization(AuthorizationPolicies.AdminOrAbove);
  }
}
