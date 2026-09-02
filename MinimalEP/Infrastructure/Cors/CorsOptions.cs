namespace MinimalEP.Infrastructure.Cors;

public sealed class CorsOptions
{
  public const string SectionName = "Cors";
  public const string PolicyName = "ApplicationCors";

  public string[] AllowedOrigins { get; init; } = [];
}
