namespace MinimalEP.Features.Auth.Login;

using MinimalEP.Features.Core;
using MinimalEP.Infrastructure.Auth;

public class LoginEndpoint : IEndpoint
{
  public IEndpointConventionBuilder MapEndpoint(IEndpointRouteBuilder builder)
  {
    return builder.MapPost(ApiRoutes.Auth.Login, async (
      LoginRequest request,
      IRequestHandler<LoginRequest, Result<LoginResponse>> handler,
      CancellationToken cancellationToken) =>
    {
      var result = await handler.HandleAsync(request, cancellationToken);

      return result.ToHttpResult(ok => TypedResults.Ok(ok));
    }).AllowAnonymous().RequireRateLimiting(RateLimitPolicies.Authentication);
  }
}
