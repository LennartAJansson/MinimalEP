namespace MinimalEP.Features.Core;

using FluentValidation;
using Microsoft.AspNetCore.Http;

public class ValidationFilter<TRequest> 
  : IEndpointFilter
    where TRequest : class
{
  public async ValueTask<object?> InvokeAsync(EndpointFilterInvocationContext context, EndpointFilterDelegate next)
  {
    // 1. Find the argument matching TRequest
    var request = context.Arguments.FirstOrDefault(x => x is TRequest) as TRequest;

    if (request is null)
    {
      return await next(context);
    }

    // 2. Resolve the validator for TRequest from DI
    var httpContext = context.HttpContext;
    var validator = httpContext.RequestServices.GetService<IValidator<TRequest>>();

    // 3. Run validation if a validator is registered
    if (validator is not null)
    {
      var validationResult = await validator.ValidateAsync(request, httpContext.RequestAborted);

      if (!validationResult.IsValid)
      {
        // Return HTTP 400 Bad Request with FluentValidation error details
        return Results.ValidationProblem(validationResult.ToDictionary());
      }
    }

    // Validation passed — continue to the next step in the pipeline
    return await next(context);
  }
}