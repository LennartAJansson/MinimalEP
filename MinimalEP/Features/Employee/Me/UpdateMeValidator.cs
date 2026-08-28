namespace MinimalEP.Features.Employee.Me;

using FluentValidation;

public class UpdateMeValidator : AbstractValidator<UpdateMeRequest>
{
  public UpdateMeValidator()
  {
    RuleFor(x => x.GivenName).NotEmpty().WithMessage("GivenName is required.");
    RuleFor(x => x.Surname).NotEmpty().WithMessage("Surname is required.");
    RuleFor(x => x.Age).InclusiveBetween(16, 100).WithMessage("Age must be between 16 and 100.");
    RuleFor(x => x.Position).NotEmpty().WithMessage("Position is required.");
    RuleFor(x => x.PhoneNumber).NotEmpty().WithMessage("PhoneNumber is required.");
    RuleFor(x => x.Street).NotEmpty().WithMessage("Street is required.");
    RuleFor(x => x.PostalCode).NotEmpty().WithMessage("PostalCode is required.");
    RuleFor(x => x.City).NotEmpty().WithMessage("City is required.");
  }
}
