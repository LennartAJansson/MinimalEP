namespace MinimalEP.Features.Core;

public abstract record Result<T>
{
  public sealed record Ok(T Value) : Result<T>;
  public sealed record NotFound() : Result<T>;
  public sealed record Conflict(string Message) : Result<T>;
}

/// <summary>Represents a successful response without a payload.</summary>
public record struct Unit
{
  public static readonly Unit Value = new();
}
