namespace MinimalEP.Features.Customer.GetCustomers;

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
      return TypedResults.Ok(((Result<GetCustomersResponse>.Ok)result).Value);
    });
  }
}
