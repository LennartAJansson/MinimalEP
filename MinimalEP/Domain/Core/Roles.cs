namespace MinimalEP.Domain.Core;

// Well-known role names, used both for Identity role seeding and authorization policies.
public static class Roles
{
  public const string SuperAdmin = "SuperAdmin";
  public const string Admin = "Admin";
  public const string User = "User";

  public static readonly string[] All = [SuperAdmin, Admin, User];
}
