namespace MinimalEP.Features.Employee.Me;

using MinimalEP.Features.Core;

public class GetMeEndpoint : IEndpoint
{
  public IEndpointConventionBuilder MapEndpoint(IEndpointRouteBuilder builder)
  {
    return builder.MapGet(ApiRoutes.Employees.Me, async (
      IRequestHandler<GetMeRequest, Result<GetMeResponse>> handler,
      CancellationToken cancellationToken) =>
    {
      var result = await handler.HandleAsync(new GetMeRequest(), cancellationToken);

      return result.ToHttpResult(ok => TypedResults.Ok(ok));
    }).WithName(ApiRouteNames.GetMe);
  }
}
