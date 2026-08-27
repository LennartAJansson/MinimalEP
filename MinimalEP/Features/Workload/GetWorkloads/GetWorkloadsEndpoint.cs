namespace MinimalEP.Features.Workload.GetWorkloads;

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
      return TypedResults.Ok(((Result<GetWorkloadsResponse>.Ok)result).Value);
    });
  }
}
