namespace MinimalEP.Tests.Infrastructure;

using System.Security.Claims;
using System.Text.Encodings.Web;

using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

public sealed class TestAuthHandler(
  IOptionsMonitor<AuthenticationSchemeOptions> options,
  ILoggerFactory logger,
  UrlEncoder encoder)
  : AuthenticationHandler<AuthenticationSchemeOptions>(options, logger, encoder)
{
  public const string SchemeName = "Test";
  public const string RoleHeader = "X-Test-Role";
  public static readonly Guid UserId = Guid.Parse("01956b6e-f4d4-7c28-a975-8f95b97c4451");

  protected override Task<AuthenticateResult> HandleAuthenticateAsync()
  {
    if (!Request.Headers.TryGetValue(RoleHeader, out var role) || string.IsNullOrWhiteSpace(role))
      return Task.FromResult(AuthenticateResult.NoResult());

    Claim[] claims =
    [
      new("sub", UserId.ToString()),
      new(ClaimTypes.NameIdentifier, UserId.ToString()),
      new(ClaimTypes.Role, role.ToString())
    ];
    var principal = new ClaimsPrincipal(new ClaimsIdentity(claims, SchemeName));
    return Task.FromResult(AuthenticateResult.Success(new AuthenticationTicket(principal, SchemeName)));
  }
}
