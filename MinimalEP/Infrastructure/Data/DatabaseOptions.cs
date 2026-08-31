namespace MinimalEP.Infrastructure.Data;

public sealed class DatabaseOptions
{
  public const string SectionName = "Database";
  public const string ConnectionStringName = "DefaultConnection";

  public bool ApplyMigrationsOnStartup { get; init; }
}
