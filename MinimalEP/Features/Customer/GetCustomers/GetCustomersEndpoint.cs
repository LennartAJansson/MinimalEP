namespace MinimalEP.Features.Customer.GetCustomers;

using System.Diagnostics;

using MinimalEP.Features.Core;

public class GetCustomersEndpoint
  : IEndpoint
{
  public IEndpointConventionBuilder MapEndpoint(IEndpointRouteBuilder builder)
  {
    return builder.MapGet("/customers", async (
      IRequestHandler<GetCustomersRequest, Result<GetCustomersResponse>> handler,
      CancellationToken cancellationToken) =>
    {
      var result = await handler.HandleAsync(new GetCustomersRequest(), cancellationToken);

      IResult httpResult = result switch
      {
        Result<GetCustomersResponse>.Ok ok => TypedResults.Ok(ok.Value),
        _ => throw new UnreachableException()
      };
      return httpResult;
    });
  }
}
