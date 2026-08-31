namespace MinimalEP.Features.Admin.CreateEmployeeAccount;

using FluentValidation;

using MinimalEP.Domain.Core;
using MinimalEP.Domain.Model;

public class CreateEmployeeAccountValidator : AbstractValidator<CreateEmployeeAccountRequest>
{
  public CreateEmployeeAccountValidator()
  {
    RuleFor(x => x.Email).NotEmpty().EmailAddress().MaximumLength(EmployeeConstraints.EmailMaxLength);
    RuleFor(x => x.GivenName).NotEmpty().MaximumLength(EmployeeConstraints.NameMaxLength);
    RuleFor(x => x.Surname).NotEmpty().MaximumLength(EmployeeConstraints.NameMaxLength);
    RuleFor(x => x.Age).InclusiveBetween(EmployeeConstraints.MinimumAge, EmployeeConstraints.MaximumAge);
    RuleFor(x => x.Position).NotEmpty().MaximumLength(EmployeeConstraints.PositionMaxLength);
    RuleFor(x => x.PhoneNumber).NotEmpty().MaximumLength(EmployeeConstraints.PhoneNumberMaxLength);
    RuleFor(x => x.Street).NotEmpty().MaximumLength(EmployeeConstraints.StreetMaxLength);
    RuleFor(x => x.PostalCode).NotEmpty().MaximumLength(EmployeeConstraints.PostalCodeMaxLength);
    RuleFor(x => x.City).NotEmpty().MaximumLength(EmployeeConstraints.CityMaxLength);
    RuleFor(x => x.Role)
      .Must(role => Roles.All.Contains(role))
      .WithMessage($"Role must be one of: {string.Join(", ", Roles.All)}.");
  }
}
