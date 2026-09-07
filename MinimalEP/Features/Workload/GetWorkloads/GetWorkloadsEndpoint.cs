namespace MinimalEP.Features.Workload.GetWorkloads;

using MinimalEP.Features.Core;

public class GetWorkloadsEndpoint : IEndpoint
{
  public IEndpointConventionBuilder MapEndpoint(IEndpointRouteBuilder builder)
  {
    return builder.MapGet(ApiRoutes.Workloads.Collection, async (
      Guid? customerId,
      Guid? employeeId,
      int? pageSize,
      Guid? after,
      IRequestHandler<GetWorkloadsRequest, Result<GetWorkloadsResponse>> handler,
      CancellationToken cancellationToken) =>
    {
      var result = await handler.HandleAsync(new GetWorkloadsRequest(customerId, employeeId, pageSize, after), cancellationToken);

      return result.ToHttpResult(ok => TypedResults.Ok(ok));
    });
  }
}
