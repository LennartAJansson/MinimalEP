namespace MinimalEP.Features.Workload.GetWorkload;

using System.Diagnostics;

using MinimalEP.Features.Core;

public class GetWorkloadEndpoint : IEndpoint
{
  public IEndpointConventionBuilder MapEndpoint(IEndpointRouteBuilder builder)
  {
    return builder.MapGet(ApiRoutes.Workloads.ById, async (
      Guid id,
      IRequestHandler<GetWorkloadRequest, Result<GetWorkloadResponse>> handler,
      CancellationToken cancellationToken) =>
    {
      var result = await handler.HandleAsync(new GetWorkloadRequest(id), cancellationToken);

      IResult httpResult = result switch
      {
        Result<GetWorkloadResponse>.Ok ok    => TypedResults.Ok(ok.Value),
        Result<GetWorkloadResponse>.NotFound => TypedResults.NotFound(),
        _                                    => throw new UnreachableException()
      };

      return httpResult;
    }).WithName(ApiRouteNames.GetWorkload);
  }
}
