namespace MinimalEP.Features.Auth.Register;

using System.Diagnostics;

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

      IResult httpResult = result switch
      {
        Result<RegisterResponse>.Ok ok      => TypedResults.CreatedAtRoute(ok.Value, ApiRouteNames.GetMe, new { version = ApiVersions.V1RouteValue }),
        Result<RegisterResponse>.Conflict c => TypedResults.Conflict(c.Message),
        _                                   => throw new UnreachableException()
      };

      return httpResult;
    }).AllowAnonymous().RequireRateLimiting(RateLimitPolicies.Authentication);
  }
}
