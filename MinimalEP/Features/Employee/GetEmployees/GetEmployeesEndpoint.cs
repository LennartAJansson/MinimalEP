namespace MinimalEP.Features.Employee.GetEmployees;

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
      return TypedResults.Ok(((Result<GetEmployeesResponse>.Ok)result).Value);
    });
  }
}
