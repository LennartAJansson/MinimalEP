namespace MinimalEP.Features.Customer.AddCustomer;

using FluentValidation;

public class AddCustomerValidator 
  : AbstractValidator<AddCustomerRequest>
{
  public AddCustomerValidator()
  {
    RuleFor(x => x.Name).NotEmpty().WithMessage("Name is required.");
    RuleFor(x => x.Email).NotEmpty().EmailAddress().WithMessage("A valid email is required.");
  }
}
