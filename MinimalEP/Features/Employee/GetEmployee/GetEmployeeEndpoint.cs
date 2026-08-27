namespace MinimalEP.Features.Employee.GetEmployee;

using System.Diagnostics;

using MinimalEP.Features.Core;

public class GetEmployeeEndpoint : IEndpoint
{
  public IEndpointConventionBuilder MapEndpoint(IEndpointRouteBuilder builder)
  {
    return builder.MapGet("/employees/{id}", async (
      Guid id,
      IRequestHandler<GetEmployeeRequest, Result<GetEmployeeResponse>> handler,
      CancellationToken cancellationToken) =>
    {
      var result = await handler.HandleAsync(new GetEmployeeRequest(id), cancellationToken);

      IResult httpResult = result switch
      {
        Result<GetEmployeeResponse>.Ok ok    => TypedResults.Ok(ok.Value),
        Result<GetEmployeeResponse>.NotFound => TypedResults.NotFound(),
        _                                    => throw new UnreachableException()
      };

      return httpResult;
    });
  }
}
