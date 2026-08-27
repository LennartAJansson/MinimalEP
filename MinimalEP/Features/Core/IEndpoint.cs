namespace MinimalEP.Features.Core;

using FluentValidation;

using MinimalEP.Features.Customer.AddCustomer;

public interface IEndpoint
{
  IEndpointConventionBuilder MapEndpoint(IEndpointRouteBuilder builder);
}
