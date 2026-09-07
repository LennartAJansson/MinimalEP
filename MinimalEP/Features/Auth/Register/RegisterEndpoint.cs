namespace MinimalEP.Features.Auth.Register;

using MinimalEP.Features.Core;
using MinimalEP.Infrastructure.Auth;

public class RegisterEndpoint : IEndpoint
{
  public IEndpointConventionBuilder MapEndpoint(IEndpointRouteBuilder builder)
  {
    return builder.MapPost(ApiRoutes.Auth.Register, async (
      RegisterRequest request,
      IRequestHandler<RegisterRequest, Result<RegisterResponse>> handler,
      CancellationToken cancellationToken) =>
    {
      var result = await handler.HandleAsync(request, cancellationToken);

      return result.ToHttpResult(ok => TypedResults.CreatedAtRoute(ok, ApiRouteNames.GetMe, new { version = ApiVersions.V1RouteValue }));
    }).AllowAnonymous().RequireRateLimiting(RateLimitPolicies.Authentication);
  }
}
