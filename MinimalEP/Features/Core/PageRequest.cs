namespace MinimalEP.Features.Core;

public readonly record struct PageRequest(Guid? After, int PageSize)
{
  public const int DefaultPageSize = 50;
  public const int MaxPageSize = 100;
  public const int MinPageSize = 1;
  public const int LookaheadSize = 1;

  public static PageRequest Create(Guid? after, int? pageSize) =>
    new(after, Math.Clamp(pageSize ?? DefaultPageSize, MinPageSize, MaxPageSize));
}
