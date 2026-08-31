namespace MinimalEP.Infrastructure.Auth;

public sealed class BootstrapAdminOptions
{
  public const string SectionName = "BootstrapAdmin";

  public bool Enabled { get; init; }
  public string Email { get; init; } = string.Empty;
  public string Password { get; init; } = string.Empty;
  public string GivenName { get; init; } = string.Empty;
  public string Surname { get; init; } = string.Empty;
  public int Age { get; init; }
  public string Position { get; init; } = string.Empty;
  public string PhoneNumber { get; init; } = string.Empty;
  public string Street { get; init; } = string.Empty;
  public string PostalCode { get; init; } = string.Empty;
  public string City { get; init; } = string.Empty;
}
