namespace MinimalEP.Features.Employee.UpdateEmployee;

using System.Diagnostics;

using MinimalEP.Features.Core;
using MinimalEP.Infrastructure.Auth;

public class UpdateEmployeeEndpoint : IEndpoint
{
  public IEndpointConventionBuilder MapEndpoint(IEndpointRouteBuilder builder)
  {
    return builder.MapPut(ApiRoutes.Employees.ById, async (
      Guid id,
      UpdateEmployeeRequest request,
      IRequestHandler<UpdateEmployeeRequest, Result<UpdateEmployeeResponse>> handler,
      CancellationToken cancellationToken) =>
    {
      var result = await handler.HandleAsync(request with { Id = id }, cancellationToken);

      IResult httpResult = result switch
      {
        Result<UpdateEmployeeResponse>.Ok ok    => TypedResults.Ok(ok.Value),
        Result<UpdateEmployeeResponse>.NotFound => TypedResults.NotFound(),
        Result<UpdateEmployeeResponse>.Conflict c => TypedResults.Conflict(c.Message),
        _                                       => throw new UnreachableException()
      };

      return httpResult;
    }).RequireAuthorization(AuthorizationPolicies.AdminOrAbove);
  }
}
