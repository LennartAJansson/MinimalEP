namespace MinimalEP.Features.Employee.AddEmployee;

using FluentValidation;

public class AddEmployeeValidator : AbstractValidator<AddEmployeeRequest>
{
  public AddEmployeeValidator()
  {
    RuleFor(x => x.Name).NotEmpty().WithMessage("Name is required.");
    RuleFor(x => x.Age).InclusiveBetween(16, 100).WithMessage("Age must be between 16 and 100.");
    RuleFor(x => x.Position).NotEmpty().WithMessage("Position is required.");
  }
}
