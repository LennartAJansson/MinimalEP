namespace MinimalEP.Features.Admin.CreateEmployeeAccount;

using FluentValidation;

using MinimalEP.Domain.Core;

public class CreateEmployeeAccountValidator : AbstractValidator<CreateEmployeeAccountRequest>
{
  public CreateEmployeeAccountValidator()
  {
    RuleFor(x => x.Email).NotEmpty().EmailAddress();
    RuleFor(x => x.GivenName).NotEmpty();
    RuleFor(x => x.Surname).NotEmpty();
    RuleFor(x => x.Age).InclusiveBetween(16, 100);
    RuleFor(x => x.Position).NotEmpty();
    RuleFor(x => x.PhoneNumber).NotEmpty();
    RuleFor(x => x.Street).NotEmpty();
    RuleFor(x => x.PostalCode).NotEmpty();
    RuleFor(x => x.City).NotEmpty();
    RuleFor(x => x.Role)
      .Must(role => Roles.All.Contains(role))
      .WithMessage($"Role must be one of: {string.Join(", ", Roles.All)}.");
  }
}
