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
      // 1. Registrera alla FluentValidation-validators automatiskt
      services.AddValidatorsFromAssemblyContaining(type);

      // 2. Registrera alla IRequestHandler automatiskt
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

      // 3.Registrera alla IEndpoint-implementeringar automatiskt
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
      var sp = scope.ServiceProvider;

      IEnumerable<IEndpoint> endpoints = scope.ServiceProvider
          .GetRequiredService<IEnumerable<IEndpoint>>();

      IEndpointRouteBuilder builder = routeGroupBuilder is null
        ? application
        : routeGroupBuilder;

      foreach (IEndpoint endpoint in endpoints)
      {
        // 1. Mappa endpointen och fånga upp konventionerna
        var conventionBuilder = endpoint.MapEndpoint(builder);

        // 2. Hitta vilken typ av RequestHandler denna endpoint använder via reflektion
        var endpointType = endpoint.GetType();

        // Vi letar efter metoder eller fält, men det absolut säkraste och enklaste sättet 
        // är att titta på vilka IRequestHandler som finns i projektet som matchar endpointens logik.
        // Ett ännu smidigare sätt är att titta på det gränssnitt endpointen har, 
        // eller skanna endpoint-klassens generiska parametrar om du i framtiden gör den generisk.

        // För att göra det helt automatiskt baserat på din nuvarande struktur:
        var handlerInterface = endpointType.Assembly.DefinedTypes
            .Where(t => t is { IsInterface: false, IsAbstract: false })
            .SelectMany(t => t.ImplementedInterfaces)
            .FirstOrDefault(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IRequestHandler<,>));
        if (handlerInterface is not null)
        {
          // Hämta ut TRequest (första generiska argumentet från IRequestHandler<TRequest, TResponse>)
          var requestType = handlerInterface.GetGenericArguments()[0];

          // Bygg typen för vårt generiska filter: ValidationFilter<TRequest>
          var filterType = typeof(ValidationFilter<>).MakeGenericType(requestType);

          // Lägg till filtret i efterhand på denna endpoint helt automatiskt!
          conventionBuilder.Add(endpointBuilder =>
          {
            endpointBuilder.FilterFactories.Add((factoryContext, next) =>
            {
              // Skapa en instans av filtret - ValidationFilter har inga dependencies så vi kan skapa direkt
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