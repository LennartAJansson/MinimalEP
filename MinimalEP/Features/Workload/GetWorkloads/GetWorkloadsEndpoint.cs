namespace MinimalEP.Features.Workload.GetWorkloads;

using System.Diagnostics;

using MinimalEP.Features.Core;

public class GetWorkloadsEndpoint : IEndpoint
{
  public IEndpointConventionBuilder MapEndpoint(IEndpointRouteBuilder builder)
  {
    return builder.MapGet("/workloads", async (
      Guid? customerId,
      Guid? employeeId,
      IRequestHandler<GetWorkloadsRequest, Result<GetWorkloadsResponse>> handler,
      CancellationToken cancellationToken) =>
    {
      var result = await handler.HandleAsync(new GetWorkloadsRequest(customerId, employeeId), cancellationToken);

      IResult httpResult = result switch
      {
        Result<GetWorkloadsResponse>.Ok ok => TypedResults.Ok(ok.Value),
        _ => throw new UnreachableException()
      };
      return httpResult;
    });
  }
}
