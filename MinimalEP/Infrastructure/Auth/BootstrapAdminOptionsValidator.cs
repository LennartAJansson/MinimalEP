namespace MinimalEP.Infrastructure.Auth;

using System.ComponentModel.DataAnnotations;

using Microsoft.Extensions.Options;

using MinimalEP.Domain.Model;

public sealed class BootstrapAdminOptionsValidator : IValidateOptions<BootstrapAdminOptions>
{
  public ValidateOptionsResult Validate(string? name, BootstrapAdminOptions options)
  {
    if (!options.Enabled)
      return ValidateOptionsResult.Success;

    var missing = new List<string>();
    AddIfMissing(options.Email, nameof(options.Email), missing);
    AddIfMissing(options.Password, nameof(options.Password), missing);
    AddIfMissing(options.GivenName, nameof(options.GivenName), missing);
    AddIfMissing(options.Surname, nameof(options.Surname), missing);
    AddIfMissing(options.Position, nameof(options.Position), missing);
    AddIfMissing(options.PhoneNumber, nameof(options.PhoneNumber), missing);
    AddIfMissing(options.Street, nameof(options.Street), missing);
    AddIfMissing(options.PostalCode, nameof(options.PostalCode), missing);
    AddIfMissing(options.City, nameof(options.City), missing);

    if (!new EmailAddressAttribute().IsValid(options.Email))
      missing.Add($"{nameof(options.Email)} must be a valid email address");

    AddIfTooLong(options.Email, EmployeeConstraints.EmailMaxLength, nameof(options.Email), missing);
    AddIfTooLong(options.GivenName, EmployeeConstraints.NameMaxLength, nameof(options.GivenName), missing);
    AddIfTooLong(options.Surname, EmployeeConstraints.NameMaxLength, nameof(options.Surname), missing);
    AddIfTooLong(options.Position, EmployeeConstraints.PositionMaxLength, nameof(options.Position), missing);
    AddIfTooLong(options.PhoneNumber, EmployeeConstraints.PhoneNumberMaxLength, nameof(options.PhoneNumber), missing);
    AddIfTooLong(options.Street, EmployeeConstraints.StreetMaxLength, nameof(options.Street), missing);
    AddIfTooLong(options.PostalCode, EmployeeConstraints.PostalCodeMaxLength, nameof(options.PostalCode), missing);
    AddIfTooLong(options.City, EmployeeConstraints.CityMaxLength, nameof(options.City), missing);

    if (options.Age is < EmployeeConstraints.MinimumAge or > EmployeeConstraints.MaximumAge)
      missing.Add($"{nameof(options.Age)} must be between {EmployeeConstraints.MinimumAge} and {EmployeeConstraints.MaximumAge}");

    if (options.Password.Length < AuthDefaults.PasswordMinimumLength)
      missing.Add($"{nameof(options.Password)} must be at least {AuthDefaults.PasswordMinimumLength} characters");

    return missing.Count == 0
      ? ValidateOptionsResult.Success
      : ValidateOptionsResult.Fail(missing);
  }

  private static void AddIfMissing(string value, string propertyName, ICollection<string> failures)
  {
    if (string.IsNullOrWhiteSpace(value))
      failures.Add($"{propertyName} is required");
  }

  private static void AddIfTooLong(string value, int maximumLength, string propertyName, ICollection<string> failures)
  {
    if (value.Length > maximumLength)
      failures.Add($"{propertyName} must not exceed {maximumLength} characters");
  }
}
