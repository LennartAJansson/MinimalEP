namespace MinimalEP.Features.Workload.AddWorkload;

using System.Diagnostics;

using MinimalEP.Features.Core;

public class AddWorkloadEndpoint : IEndpoint
{
  public IEndpointConventionBuilder MapEndpoint(IEndpointRouteBuilder builder)
  {
    return builder.MapPost("/workloads", async (
      AddWorkloadRequest request,
      IRequestHandler<AddWorkloadRequest, Result<AddWorkloadResponse>> handler,
      CancellationToken cancellationToken) =>
    {
      var result = await handler.HandleAsync(request, cancellationToken);

      IResult httpResult = result switch
      {
        Result<AddWorkloadResponse>.Ok ok      => TypedResults.Created($"/workloads/{ok.Value.Id}", ok.Value),
        Result<AddWorkloadResponse>.Conflict c => TypedResults.Conflict(c.Message),
        _                                      => throw new UnreachableException()
      };

      return httpResult;
    });
  }
}
