namespace MinimalEP.Features.Core;

public interface IRequestHandler<TRequest, TResponse>
{
  Task<TResponse> HandleAsync(TRequest request, CancellationToken cancellationToken);
}
