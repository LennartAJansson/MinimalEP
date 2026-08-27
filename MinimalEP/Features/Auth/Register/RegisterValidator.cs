namespace MinimalEP.Features.Auth.Register;

using FluentValidation;

public class RegisterValidator : AbstractValidator<RegisterRequest>
{
  public RegisterValidator()
  {
    RuleFor(x => x.Email).NotEmpty().EmailAddress().WithMessage("A valid email is required.");
    RuleFor(x => x.Password).NotEmpty().MinimumLength(8).WithMessage("Password must be at least 8 characters.");
    RuleFor(x => x.Name).NotEmpty().WithMessage("Name is required.");
    RuleFor(x => x.Age).InclusiveBetween(16, 100).WithMessage("Age must be between 16 and 100.");
    RuleFor(x => x.Position).NotEmpty().WithMessage("Position is required.");
  }
}
