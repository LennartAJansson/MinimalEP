namespace MinimalEP.Features.Employee.UpdateEmployee;

using System.Diagnostics;

using MinimalEP.Features.Core;

public class UpdateEmployeeEndpoint : IEndpoint
{
  public IEndpointConventionBuilder MapEndpoint(IEndpointRouteBuilder builder)
  {
    return builder.MapPut("/employees/{id}", async (
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
        _                                       => throw new UnreachableException()
      };

      return httpResult;
    });
  }
}
