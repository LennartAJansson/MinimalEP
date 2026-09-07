namespace MinimalEP.Features.Core;

using FluentValidation;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

public class ValidationFilter<TRequest> 
  : IEndpointFilter
    where TRequest : class
{
  public async ValueTask<object?> InvokeAsync(EndpointFilterInvocationContext context, EndpointFilterDelegate next)
  {
    // 1. Find the argument matching TRequest

    if (context.Arguments.FirstOrDefault(x => x is TRequest) is not TRequest request)
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
        var problem = new HttpValidationProblemDetails(validationResult.ToDictionary())
        {
          Status = StatusCodes.Status400BadRequest,
          Title = "Validation failed.",
          Detail = "One or more validation errors occurred."
        };
        problem.Extensions["code"] = ResultErrorCodes.ValidationFailed;

        return TypedResults.Problem(problem);
      }
    }

    // Validation passed — continue to the next step in the pipeline
    return await next(context);
  }
}
