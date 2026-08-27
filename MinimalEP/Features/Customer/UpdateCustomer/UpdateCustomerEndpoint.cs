namespace MinimalEP.Features.Customer.UpdateCustomer;

using System.Diagnostics;

using MinimalEP.Features.Core;

public class UpdateCustomerEndpoint
  : IEndpoint
{
  public IEndpointConventionBuilder MapEndpoint(IEndpointRouteBuilder builder)
  {
    return builder.MapPut("/customers/{id}", async (
      Guid id,
      UpdateCustomerRequest request,
      IRequestHandler<UpdateCustomerRequest, Result<UpdateCustomerResponse>> handler,
      CancellationToken cancellationToken) =>
    {
      // Slå ihop route-id med body-datan innan vi skickar till handler
      var result = await handler.HandleAsync(request with { Id = id }, cancellationToken);

      IResult httpResult = result switch
      {
        Result<UpdateCustomerResponse>.Ok ok    => TypedResults.Ok(ok.Value),
        Result<UpdateCustomerResponse>.NotFound => TypedResults.NotFound(),
        _                                       => throw new UnreachableException()
      };

      return httpResult;
    });
  }
}
