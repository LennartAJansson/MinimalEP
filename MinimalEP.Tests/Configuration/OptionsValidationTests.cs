namespace MinimalEP.Tests.Configuration;

using MinimalEP.Infrastructure.Auth;

public sealed class OptionsValidationTests
{
  [Fact]
  public void Enabled_bootstrap_requires_complete_values()
  {
    var validator = new BootstrapAdminOptionsValidator();

    var result = validator.Validate(null, new BootstrapAdminOptions { Enabled = true });

    Assert.True(result.Failed);
  }

  [Fact]
  public void Disabled_bootstrap_allows_empty_values()
  {
    var validator = new BootstrapAdminOptionsValidator();

    var result = validator.Validate(null, new BootstrapAdminOptions { Enabled = false });

    Assert.True(result.Succeeded);
  }
}
