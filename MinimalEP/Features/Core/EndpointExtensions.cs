namespace MinimalEP.Features.Core;

using System.Reflection;

using FluentValidation;

using Microsoft.Extensions.DependencyInjection.Extensions;

public static class EndpointExtensions
{
  extension(IServiceCollection services)
  {
    public IServiceCollection AddEndpoints(Type type)
    {
      // 1. Auto-register all FluentValidation validators
      services.AddValidatorsFromAssemblyContaining(type);

      // 2. Auto-register all IRequestHandler implementations
      var handlerDescriptors = type.Assembly
        .DefinedTypes
        .Where(type => type is { IsInterface: false, IsAbstract: false })
        .SelectMany(type => type.ImplementedInterfaces, (type, interfaceType) => new { type, interfaceType })
        .Where(x => x.interfaceType.IsGenericType &&
                    x.interfaceType.GetGenericTypeDefinition() == typeof(IRequestHandler<,>))
        .Select(x => ServiceDescriptor.Transient(x.interfaceType, x.type))
        .ToArray();
      foreach (var descriptor in handlerDescriptors)
      {
        services.Add(descriptor);
      }

      // 3. Auto-register all IEndpoint implementations
      ServiceDescriptor[] endpointDescriptors = type.Assembly
          .DefinedTypes
          .Where(type => type is { IsInterface: false, IsAbstract: false } &&
                         type.IsAssignableTo(typeof(IEndpoint)))
          .Select(type => ServiceDescriptor.Transient(typeof(IEndpoint), type))
          .ToArray();
      services.TryAddEnumerable(endpointDescriptors);

      return services;
    }
  }

  extension(WebApplication application)
  {
    public WebApplication MapEndpoints(RouteGroupBuilder? routeGroupBuilder = null)
    {
      using var scope = application.Services.CreateScope();

      IEnumerable<IEndpoint> endpoints = scope.ServiceProvider
          .GetRequiredService<IEnumerable<IEndpoint>>();

      IEndpointRouteBuilder builder = routeGroupBuilder is null
        ? application
        : routeGroupBuilder;

      foreach (IEndpoint endpoint in endpoints)
      {
        // Map the endpoint and capture the convention builder
        var conventionBuilder = endpoint.MapEndpoint(builder);

        // Locate the IRequestHandler<TRequest,TResponse> interface that lives in the
        // same namespace as the endpoint — this ensures each endpoint gets its own
        // ValidationFilter<TRequest>, not one picked arbitrarily from the assembly.
        var endpointNamespace = endpoint.GetType().Namespace;

        var handlerInterface = endpoint.GetType().Assembly.DefinedTypes
            .Where(t => t is { IsInterface: false, IsAbstract: false } &&
                        t.Namespace == endpointNamespace)
            .SelectMany(t => t.ImplementedInterfaces)
            .FirstOrDefault(i => i.IsGenericType &&
                                 i.GetGenericTypeDefinition() == typeof(IRequestHandler<,>));

        if (handlerInterface is not null)
        {
          // Extract TRequest (first generic argument of IRequestHandler<TRequest, TResponse>)
          var requestType = handlerInterface.GetGenericArguments()[0];

          // Build the closed generic ValidationFilter<TRequest>
          var filterType = typeof(ValidationFilter<>).MakeGenericType(requestType);

          conventionBuilder.Add(endpointBuilder =>
          {
            endpointBuilder.FilterFactories.Add((_, next) =>
            {
              var filter = (IEndpointFilter)Activator.CreateInstance(filterType)!;
              return invocationContext => filter.InvokeAsync(invocationContext, next);
            });
          });
        }
      }

      return application;
    }
  }
}
