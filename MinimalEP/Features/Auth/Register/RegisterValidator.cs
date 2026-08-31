namespace MinimalEP.Features.Auth.Register;

using FluentValidation;

using MinimalEP.Domain.Model;
using MinimalEP.Infrastructure.Auth;

public class RegisterValidator : AbstractValidator<RegisterRequest>
{
  public RegisterValidator()
  {
    RuleFor(x => x.Email).NotEmpty().EmailAddress().MaximumLength(EmployeeConstraints.EmailMaxLength);
    RuleFor(x => x.Password).NotEmpty().MinimumLength(AuthDefaults.PasswordMinimumLength);
    RuleFor(x => x.GivenName).NotEmpty().MaximumLength(EmployeeConstraints.NameMaxLength);
    RuleFor(x => x.Surname).NotEmpty().MaximumLength(EmployeeConstraints.NameMaxLength);
    RuleFor(x => x.Age).InclusiveBetween(EmployeeConstraints.MinimumAge, EmployeeConstraints.MaximumAge);
    RuleFor(x => x.Position).NotEmpty().MaximumLength(EmployeeConstraints.PositionMaxLength);
    RuleFor(x => x.PhoneNumber).NotEmpty().MaximumLength(EmployeeConstraints.PhoneNumberMaxLength);
    RuleFor(x => x.Street).NotEmpty().MaximumLength(EmployeeConstraints.StreetMaxLength);
    RuleFor(x => x.PostalCode).NotEmpty().MaximumLength(EmployeeConstraints.PostalCodeMaxLength);
    RuleFor(x => x.City).NotEmpty().MaximumLength(EmployeeConstraints.CityMaxLength);
  }
}
