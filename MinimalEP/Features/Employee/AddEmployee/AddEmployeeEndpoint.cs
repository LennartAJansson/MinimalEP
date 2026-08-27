namespace MinimalEP.Features.Employee.AddEmployee;

using System.Diagnostics;

using MinimalEP.Features.Core;

public class AddEmployeeEndpoint : IEndpoint
{
  public IEndpointConventionBuilder MapEndpoint(IEndpointRouteBuilder builder)
  {
    return builder.MapPost("/employees", async (
      AddEmployeeRequest request,
      IRequestHandler<AddEmployeeRequest, Result<AddEmployeeResponse>> handler,
      CancellationToken cancellationToken) =>
    {
      var result = await handler.HandleAsync(request, cancellationToken);

      IResult httpResult = result switch
      {
        Result<AddEmployeeResponse>.Ok ok => TypedResults.Created($"/employees/{ok.Value.Id}", ok.Value),
        _                                 => throw new UnreachableException()
      };

      return httpResult;
    });
  }
}
