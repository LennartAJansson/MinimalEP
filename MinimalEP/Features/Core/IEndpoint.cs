namespace MinimalEP.Features.Core;

public interface IEndpoint
{
  IEndpointConventionBuilder MapEndpoint(IEndpointRouteBuilder builder);
}
