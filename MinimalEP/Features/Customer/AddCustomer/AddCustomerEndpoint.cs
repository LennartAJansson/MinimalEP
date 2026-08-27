namespace MinimalEP.Features.Customer.AddCustomer;

using System.Diagnostics;

using MinimalEP.Features.Core;

public class AddCustomerEndpoint 
  : IEndpoint
{
    public IEndpointConventionBuilder MapEndpoint(IEndpointRouteBuilder builder)
    {
      return builder.MapPost("/customers", async (
        AddCustomerRequest request,
        IRequestHandler<AddCustomerRequest, Result<AddCustomerResponse>> handler,
        CancellationToken cancellationToken) =>
      {
        var result = await handler.HandleAsync(request, cancellationToken);

        IResult httpResult = result switch
        {
          Result<AddCustomerResponse>.Ok ok       => TypedResults.Created($"/customers/{ok.Value.Id}", ok.Value),
          Result<AddCustomerResponse>.Conflict c  => TypedResults.Conflict(c.Message),
          _                                       => throw new UnreachableException()
        };

        return httpResult;
      });
    }
}
