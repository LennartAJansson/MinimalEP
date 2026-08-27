namespace MinimalEP.Features.Workload.StopWorkload;

using System.Diagnostics;

using MinimalEP.Features.Core;

public class StopWorkloadEndpoint : IEndpoint
{
  public IEndpointConventionBuilder MapEndpoint(IEndpointRouteBuilder builder)
  {
    return builder.MapPatch("/workloads/{id}/stop", async (
      Guid id,
      StopWorkloadRequest request,
      IRequestHandler<StopWorkloadRequest, Result<StopWorkloadResponse>> handler,
      CancellationToken cancellationToken) =>
    {
      var result = await handler.HandleAsync(request with { Id = id }, cancellationToken);

      IResult httpResult = result switch
      {
        Result<StopWorkloadResponse>.Ok ok      => TypedResults.Ok(ok.Value),
        Result<StopWorkloadResponse>.NotFound   => TypedResults.NotFound(),
        Result<StopWorkloadResponse>.Conflict c => TypedResults.Conflict(c.Message),
        _                                       => throw new UnreachableException()
      };

      return httpResult;
    });
  }
}
