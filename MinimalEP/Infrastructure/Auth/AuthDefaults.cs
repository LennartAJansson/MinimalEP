namespace MinimalEP.Infrastructure.Auth;

public static class AuthDefaults
{
  public const int PasswordMinimumLength = 8;
  public const int MaxFailedAccessAttempts = 5;
  public const int LockoutMinutes = 15;
  public const int RateLimitPermitCount = 10;
  public const int RateLimitWindowMinutes = 1;
  public const int RefreshTokenSizeBytes = 64;
  public const int TemporaryPasswordRandomBytes = 16;
}

public static class EmployeeClaimNames
{
  public const string Name = "name";
  public const string Age = "age";
  public const string Position = "position";
}
