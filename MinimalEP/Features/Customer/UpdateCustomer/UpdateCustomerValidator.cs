namespace MinimalEP.Features.Customer.UpdateCustomer;

using FluentValidation;

using MinimalEP.Domain.Model;

public class UpdateCustomerValidator
  : AbstractValidator<UpdateCustomerRequest>
{
  public UpdateCustomerValidator()
  {
    RuleFor(x => x.Name).NotEmpty().MaximumLength(CustomerConstraints.NameMaxLength).WithMessage("Name is required and must fit the supported length.");
    RuleFor(x => x.Email).NotEmpty().EmailAddress().MaximumLength(CustomerConstraints.EmailMaxLength).WithMessage("A valid email is required and must fit the supported length.");
    RuleFor(x => x.RowVersion).NotEmpty();
  }
}
