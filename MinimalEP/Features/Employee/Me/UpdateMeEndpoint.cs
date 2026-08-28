namespace MinimalEP.Features.Employee.Me;

using System.Diagnostics;

using MinimalEP.Features.Core;

public class UpdateMeEndpoint : IEndpoint
{
  public IEndpointConventionBuilder MapEndpoint(IEndpointRouteBuilder builder)
  {
    return builder.MapPut("/me", async (
      UpdateMeRequest request,
      IRequestHandler<UpdateMeRequest, Result<GetMeResponse>> handler,
      CancellationToken cancellationToken) =>
    {
      var result = await handler.HandleAsync(request, cancellationToken);

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
