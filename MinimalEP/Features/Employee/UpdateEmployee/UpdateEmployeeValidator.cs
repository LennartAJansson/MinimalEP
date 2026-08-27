namespace MinimalEP.Features.Employee.UpdateEmployee;

using FluentValidation;

public class UpdateEmployeeValidator : AbstractValidator<UpdateEmployeeRequest>
{
  public UpdateEmployeeValidator()
  {
    RuleFor(x => x.Name).NotEmpty().WithMessage("Name is required.");
    RuleFor(x => x.Age).InclusiveBetween(16, 100).WithMessage("Age must be between 16 and 100.");
    RuleFor(x => x.Position).NotEmpty().WithMessage("Position is required.");
  }
}
