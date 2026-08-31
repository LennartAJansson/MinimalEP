namespace MinimalEP.Features.Employee.GetEmployees;

using System.Diagnostics;

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

      IResult httpResult = result switch
      {
        Result<GetEmployeesResponse>.Ok ok => TypedResults.Ok(ok.Value),
        _ => throw new UnreachableException()
      };
      return httpResult;
    }).RequireAuthorization(AuthorizationPolicies.AdminOrAbove);
  }
}
