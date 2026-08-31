namespace MinimalEP.Features.Auth.RefreshToken;

using System.Diagnostics;

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

      IResult httpResult = result switch
      {
        Result<RefreshTokenResponse>.Ok ok      => TypedResults.Ok(ok.Value),
        Result<RefreshTokenResponse>.Conflict c => TypedResults.Conflict(c.Message),
        _                                       => throw new UnreachableException()
      };

      return httpResult;
    }).AllowAnonymous().RequireRateLimiting(RateLimitPolicies.Authentication);
  }
}
