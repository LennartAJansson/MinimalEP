namespace MinimalEP.Features.Auth.Login;

using FluentValidation;

using MinimalEP.Domain.Model;

public class LoginValidator : AbstractValidator<LoginRequest>
{
  public LoginValidator()
  {
    RuleFor(x => x.Email).NotEmpty().EmailAddress().MaximumLength(EmployeeConstraints.EmailMaxLength);
    RuleFor(x => x.Password).NotEmpty();
  }
}
