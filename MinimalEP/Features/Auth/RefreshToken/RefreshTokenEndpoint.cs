namespace MinimalEP.Features.Auth.RefreshToken;

using MinimalEP.Features.Core;
using MinimalEP.Infrastructure.Auth;

public class RefreshTokenEndpoint : IEndpoint
{
  public IEndpointConventionBuilder MapEndpoint(IEndpointRouteBuilder builder)
  {
    return builder.MapPost(ApiRoutes.Auth.Refresh, async (
      RefreshTokenRequest request,
      IRequestHandler<RefreshTokenRequest, Result<RefreshTokenResponse>> handler,
      CancellationToken cancellationToken) =>
    {
      var result = await handler.HandleAsync(request, cancellationToken);

      return result.ToHttpResult(ok => TypedResults.Ok(ok));
    }).AllowAnonymous().RequireRateLimiting(RateLimitPolicies.Authentication);
  }
}
