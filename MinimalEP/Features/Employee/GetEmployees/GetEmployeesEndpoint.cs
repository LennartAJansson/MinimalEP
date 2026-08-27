namespace MinimalEP.Features.Employee.GetEmployees;

using System.Diagnostics;

using MinimalEP.Features.Core;

public class GetEmployeesEndpoint : IEndpoint
{
  public IEndpointConventionBuilder MapEndpoint(IEndpointRouteBuilder builder)
  {
    return builder.MapGet("/employees", async (
      IRequestHandler<GetEmployeesRequest, Result<GetEmployeesResponse>> handler,
      CancellationToken cancellationToken) =>
    {
      var result = await handler.HandleAsync(new GetEmployeesRequest(), cancellationToken);

      IResult httpResult = result switch
      {
        Result<GetEmployeesResponse>.Ok ok => TypedResults.Ok(ok.Value),
        _ => throw new UnreachableException()
      };
      return httpResult;
    });
  }
}
