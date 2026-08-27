namespace MinimalEP.Features.Core;

public abstract record Result<T>
{
  public sealed record Ok(T Value) : Result<T>;
  public sealed record NotFound() : Result<T>;
  public sealed record Conflict(string Message) : Result<T>;
}

/// <summary>Används som TResponse när en handler inte returnerar någon data, t.ex. Delete.</summary>
public record struct Unit
{
  public static readonly Unit Value = new();
}
