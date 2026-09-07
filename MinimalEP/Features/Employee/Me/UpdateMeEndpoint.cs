namespace MinimalEP.Features.Employee.Me;

using MinimalEP.Features.Core;

public class UpdateMeEndpoint : IEndpoint
{
  public IEndpointConventionBuilder MapEndpoint(IEndpointRouteBuilder builder)
  {
    return builder.MapPut(ApiRoutes.Employees.Me, async (
      UpdateMeRequest request,
      IRequestHandler<UpdateMeRequest, Result<GetMeResponse>> handler,
      CancellationToken cancellationToken) =>
    {
      var result = await handler.HandleAsync(request, cancellationToken);

      return result.ToHttpResult(ok => TypedResults.Ok(ok));
    });
  }
}
