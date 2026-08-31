namespace MinimalEP.Features.Workload.StartWorkload;

using System.Diagnostics;

using MinimalEP.Features.Core;

public class StartWorkloadEndpoint : IEndpoint
{
  public IEndpointConventionBuilder MapEndpoint(IEndpointRouteBuilder builder)
  {
    return builder.MapPost(ApiRoutes.Workloads.Start, async (
      StartWorkloadRequest request,
      IRequestHandler<StartWorkloadRequest, Result<StartWorkloadResponse>> handler,
      CancellationToken cancellationToken) =>
    {
      var result = await handler.HandleAsync(request, cancellationToken);

      IResult httpResult = result switch
      {
        Result<StartWorkloadResponse>.Ok ok      => TypedResults.CreatedAtRoute(ok.Value, ApiRouteNames.GetWorkload, new { version = ApiVersions.V1RouteValue, id = ok.Value.Id }),
        Result<StartWorkloadResponse>.Conflict c => TypedResults.Conflict(c.Message),
        _                                        => throw new UnreachableException()
      };

      return httpResult;
    });
  }
}
