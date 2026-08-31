namespace MinimalEP.Features.Workload.UpdateWorkload;

using System.Diagnostics;

using MinimalEP.Features.Core;

public class UpdateWorkloadEndpoint : IEndpoint
{
  public IEndpointConventionBuilder MapEndpoint(IEndpointRouteBuilder builder)
  {
    return builder.MapPut(ApiRoutes.Workloads.ById, async (
      Guid id,
      UpdateWorkloadRequest request,
      IRequestHandler<UpdateWorkloadRequest, Result<UpdateWorkloadResponse>> handler,
      CancellationToken cancellationToken) =>
    {
      var result = await handler.HandleAsync(request with { Id = id }, cancellationToken);

      IResult httpResult = result switch
      {
        Result<UpdateWorkloadResponse>.Ok ok    => TypedResults.Ok(ok.Value),
        Result<UpdateWorkloadResponse>.NotFound => TypedResults.NotFound(),
        Result<UpdateWorkloadResponse>.Conflict c => TypedResults.Conflict(c.Message),
        _                                       => throw new UnreachableException()
      };

      return httpResult;
    });
  }
}
