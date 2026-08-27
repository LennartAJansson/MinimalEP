namespace MinimalEP.Features.Auth.Login;

using System.Diagnostics;

using MinimalEP.Features.Core;

public class LoginEndpoint : IEndpoint
{
  public IEndpointConventionBuilder MapEndpoint(IEndpointRouteBuilder builder)
  {
    return builder.MapPost("/auth/login", async (
      LoginRequest request,
      IRequestHandler<LoginRequest, Result<LoginResponse>> handler,
      CancellationToken cancellationToken) =>
    {
      var result = await handler.HandleAsync(request, cancellationToken);

      IResult httpResult = result switch
      {
        Result<LoginResponse>.Ok ok    => TypedResults.Ok(ok.Value),
        Result<LoginResponse>.NotFound => TypedResults.Unauthorized(),
        _                              => throw new UnreachableException()
      };

      return httpResult;
    }).AllowAnonymous();
  }
}
