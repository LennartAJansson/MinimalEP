namespace MinimalEP.Features.Customer.GetCustomer;

using System.Diagnostics;

using MinimalEP.Features.Core;

public class GetCustomerEndpoint
  : IEndpoint
{
  public IEndpointConventionBuilder MapEndpoint(IEndpointRouteBuilder builder)
  {
    return builder.MapGet("/customers/{id}", async (
      Guid id,
      IRequestHandler<GetCustomerRequest, Result<GetCustomerResponse>> handler,
      CancellationToken cancellationToken) =>
    {
      var result = await handler.HandleAsync(new GetCustomerRequest(id), cancellationToken);

      IResult httpResult = result switch
      {
        Result<GetCustomerResponse>.Ok ok    => TypedResults.Ok(ok.Value),
        Result<GetCustomerResponse>.NotFound => TypedResults.NotFound(),
        _                                    => throw new UnreachableException()
      };

      return httpResult;
    });
  }
}
