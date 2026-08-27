namespace MinimalEP.Features.Workload.UpdateWorkload;

using System.Diagnostics;

using MinimalEP.Features.Core;

public class UpdateWorkloadEndpoint : IEndpoint
{
  public IEndpointConventionBuilder MapEndpoint(IEndpointRouteBuilder builder)
  {
    return builder.MapPut("/workloads/{id}", async (
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
        _                                       => throw new UnreachableException()
      };

      return httpResult;
    });
  }
}
