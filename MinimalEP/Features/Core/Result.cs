namespace MinimalEP.Features.Core;

public static class ResultErrorCodes
{
  public const string NotFound = "not_found";
  public const string Conflict = "conflict";
  public const string ValidationFailed = "validation_failed";
  public const string Unauthorized = "unauthorized";
  public const string Forbidden = "forbidden";
}

public abstract record Result<T>
{
  public sealed record Ok(T Value) : Result<T>;
  public sealed record NotFound(string Code = ResultErrorCodes.NotFound) : Result<T>;
  public sealed record Conflict(string Message, string Code = ResultErrorCodes.Conflict) : Result<T>;
  public sealed record Validation(IReadOnlyDictionary<string, string[]> Errors, string Message = "Validation failed.", string Code = ResultErrorCodes.ValidationFailed) : Result<T>;
  public sealed record Unauthorized(string Message = "Authentication is required.", string Code = ResultErrorCodes.Unauthorized) : Result<T>;
  public sealed record Forbidden(string Message = "You are not allowed to perform this action.", string Code = ResultErrorCodes.Forbidden) : Result<T>;
}

/// <summary>Represents a successful response without a payload.</summary>
public readonly record struct Unit
{
  public static readonly Unit Value;
}
