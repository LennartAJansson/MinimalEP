namespace MinimalEP.Features.Auth.Register;

using System.Diagnostics;

using MinimalEP.Features.Core;

public class RegisterEndpoint : IEndpoint
{
  public IEndpointConventionBuilder MapEndpoint(IEndpointRouteBuilder builder)
  {
    return builder.MapPost("/auth/register", async (
      RegisterRequest request,
      IRequestHandler<RegisterRequest, Result<RegisterResponse>> handler,
      CancellationToken cancellationToken) =>
    {
      var result = await handler.HandleAsync(request, cancellationToken);

      IResult httpResult = result switch
      {
        Result<RegisterResponse>.Ok ok      => TypedResults.Created($"/auth/{ok.Value.UserId}", ok.Value),
        Result<RegisterResponse>.Conflict c => TypedResults.Conflict(c.Message),
        _                                   => throw new UnreachableException()
      };

      return httpResult;
    }).AllowAnonymous();
  }
}
