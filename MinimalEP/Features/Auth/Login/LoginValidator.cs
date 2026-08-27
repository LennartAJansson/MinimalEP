namespace MinimalEP.Features.Auth.Login;

using FluentValidation;

public class LoginValidator : AbstractValidator<LoginRequest>
{
  public LoginValidator()
  {
    RuleFor(x => x.Email).NotEmpty().EmailAddress();
    RuleFor(x => x.Password).NotEmpty();
  }
}
