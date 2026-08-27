namespace MinimalEP.Features.Core;

using FluentValidation;
using Microsoft.AspNetCore.Http;

public class ValidationFilter<TRequest> 
  : IEndpointFilter
    where TRequest : class
{
  public async ValueTask<object?> InvokeAsync(EndpointFilterInvocationContext context, EndpointFilterDelegate next)
  {
    // 1. Hitta argumentet som matchar TRequest
    var request = context.Arguments.FirstOrDefault(x => x is TRequest) as TRequest;

    if (request is null)
    {
      return await next(context);
    }

    // 2. Hämta rätt validator från DI-behållaren via HttpContext 
    var httpContext = context.HttpContext;
    var validator = httpContext.RequestServices.GetService<IValidator<TRequest>>();

    // 3. Om det finns en validator för detta objekt, kör den!
    if (validator is not null)
    {
      var validationResult = await validator.ValidateAsync(request, httpContext.RequestAborted);

      if (!validationResult.IsValid)
      {
        // Returnerar ett färdigt HTTP 400 Bad Request med FluentValidations felmeddelanden
        return Results.ValidationProblem(validationResult.ToDictionary());
      }
    }

    // Allt ok! Gå vidare till nästa steg i pipelinen (din Handler)
    return await next(context);
  }
}