namespace MinimalEP.Features.Employee.Me;

using System.Diagnostics;

using MinimalEP.Features.Core;

public class GetMeEndpoint : IEndpoint
{
  public IEndpointConventionBuilder MapEndpoint(IEndpointRouteBuilder builder)
  {
    return builder.MapGet("/me", async (
      IRequestHandler<GetMeRequest, Result<GetMeResponse>> handler,
      CancellationToken cancellationToken) =>
    {
      var result = await handler.HandleAsync(new GetMeRequest(), cancellationToken);

      IResult httpResult = result switch
      {
        Result<GetMeResponse>.Ok ok    => TypedResults.Ok(ok.Value),
        Result<GetMeResponse>.NotFound => TypedResults.NotFound(),
        _                              => throw new UnreachableException()
      };

      return httpResult;
    });
  }
}
