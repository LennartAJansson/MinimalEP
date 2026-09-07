namespace MinimalEP.Features.Core;

using System.Diagnostics;

using Microsoft.AspNetCore.Mvc;

public static class ResultHttpMapping
{
  public static IResult ToHttpResult<T>(this Result<T> result, Func<T, IResult> onOk)
  {
    IResult httpResult = result switch
    {
      Result<T>.Ok ok                 => onOk(ok.Value),
      Result<T>.NotFound notFound     => TypedResults.NotFound(CreateProblem(StatusCodes.Status404NotFound, "Not Found", "The requested resource was not found.", notFound.Code)),
      Result<T>.Conflict conflict     => TypedResults.Conflict(CreateProblem(StatusCodes.Status409Conflict, "Conflict", conflict.Message, conflict.Code)),
      Result<T>.Validation validation => TypedResults.Problem(CreateValidationProblem(validation)),
      Result<T>.Unauthorized unauthorized => TypedResults.Problem(CreateProblem(StatusCodes.Status401Unauthorized, "Unauthorized", unauthorized.Message, unauthorized.Code)),
      Result<T>.Forbidden forbidden   => TypedResults.Problem(CreateProblem(StatusCodes.Status403Forbidden, "Forbidden", forbidden.Message, forbidden.Code)),
      _                               => throw new UnreachableException()
    };

    return httpResult;
  }

  private static ProblemDetails CreateProblem(int status, string title, string detail, string code)
  {
    var problem = new ProblemDetails
    {
      Status = status,
      Title = title,
      Detail = detail
    };
    problem.Extensions["code"] = code;

    return problem;
  }

  private static HttpValidationProblemDetails CreateValidationProblem<T>(Result<T>.Validation validation)
  {
    var problem = new HttpValidationProblemDetails(validation.Errors)
    {
      Status = StatusCodes.Status400BadRequest,
      Title = "Validation failed.",
      Detail = validation.Message
    };
    problem.Extensions["code"] = validation.Code;

    return problem;
  }
}
