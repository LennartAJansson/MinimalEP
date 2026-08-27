namespace MinimalEP.Features.Employee.DeleteEmployee;

using System.Diagnostics;

using MinimalEP.Features.Core;

public class DeleteEmployeeEndpoint : IEndpoint
{
  public IEndpointConventionBuilder MapEndpoint(IEndpointRouteBuilder builder)
  {
    return builder.MapDelete("/employees/{id}", async (
      Guid id,
      IRequestHandler<DeleteEmployeeRequest, Result<Unit>> handler,
      CancellationToken cancellationToken) =>
    {
      var result = await handler.HandleAsync(new DeleteEmployeeRequest(id), cancellationToken);

      IResult httpResult = result switch
      {
        Result<Unit>.Ok       => TypedResults.NoContent(),
        Result<Unit>.NotFound => TypedResults.NotFound(),
        _                     => throw new UnreachableException()
      };

      return httpResult;
    });
  }
}
