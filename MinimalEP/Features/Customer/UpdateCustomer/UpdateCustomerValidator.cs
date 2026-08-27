namespace MinimalEP.Features.Customer.UpdateCustomer;

using FluentValidation;

public class UpdateCustomerValidator
  : AbstractValidator<UpdateCustomerRequest>
{
  public UpdateCustomerValidator()
  {
    RuleFor(x => x.Name).NotEmpty().WithMessage("Name is required.");
    RuleFor(x => x.Email).NotEmpty().EmailAddress().WithMessage("A valid email is required.");
  }
}
