namespace MinimalEP.Features.Employee.GetEmployees;

using MinimalEP.Features.Core;
using MinimalEP.Infrastructure.Auth;

public class GetEmployeesEndpoint : IEndpoint
{
  public IEndpointConventionBuilder MapEndpoint(IEndpointRouteBuilder builder)
  {
    return builder.MapGet(ApiRoutes.Employees.Collection, async (
      int? pageSize,
      Guid? after,
      IRequestHandler<GetEmployeesRequest, Result<GetEmployeesResponse>> handler,
      CancellationToken cancellationToken) =>
    {
      var result = await handler.HandleAsync(new GetEmployeesRequest(pageSize, after), cancellationToken);

      return result.ToHttpResult(ok => TypedResults.Ok(ok));
    }).RequireAuthorization(AuthorizationPolicies.AdminOrAbove);
  }
}
