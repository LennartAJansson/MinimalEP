namespace MinimalEP.Infrastructure.Auth;

public static class AuthorizationPolicies
{
  public const string SuperAdminOnly = "SuperAdminOnly";
  public const string AdminOrAbove = "AdminOrAbove";
}
