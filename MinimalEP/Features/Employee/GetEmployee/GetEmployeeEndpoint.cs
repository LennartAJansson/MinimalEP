namespace MinimalEP.Features.Employee.GetEmployee;

using MinimalEP.Features.Core;
using MinimalEP.Infrastructure.Auth;

public class GetEmployeeEndpoint : IEndpoint
{
  public IEndpointConventionBuilder MapEndpoint(IEndpointRouteBuilder builder)
  {
    return builder.MapGet(ApiRoutes.Employees.ById, async (
      Guid id,
      IRequestHandler<GetEmployeeRequest, Result<GetEmployeeResponse>> handler,
      CancellationToken cancellationToken) =>
    {
      var result = await handler.HandleAsync(new GetEmployeeRequest(id), cancellationToken);

      return result.ToHttpResult(ok => TypedResults.Ok(ok));
    }).WithName(ApiRouteNames.GetEmployee).RequireAuthorization(AuthorizationPolicies.AdminOrAbove);
  }
}
