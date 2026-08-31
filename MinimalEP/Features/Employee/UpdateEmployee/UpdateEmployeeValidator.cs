namespace MinimalEP.Features.Employee.UpdateEmployee;

using FluentValidation;

using MinimalEP.Domain.Model;

public class UpdateEmployeeValidator : AbstractValidator<UpdateEmployeeRequest>
{
  public UpdateEmployeeValidator()
  {
    RuleFor(x => x.GivenName).NotEmpty().MaximumLength(EmployeeConstraints.NameMaxLength);
    RuleFor(x => x.Surname).NotEmpty().MaximumLength(EmployeeConstraints.NameMaxLength);
    RuleFor(x => x.Age).InclusiveBetween(EmployeeConstraints.MinimumAge, EmployeeConstraints.MaximumAge);
    RuleFor(x => x.Position).NotEmpty().MaximumLength(EmployeeConstraints.PositionMaxLength);
    RuleFor(x => x.PhoneNumber).NotEmpty().MaximumLength(EmployeeConstraints.PhoneNumberMaxLength);
    RuleFor(x => x.Street).NotEmpty().MaximumLength(EmployeeConstraints.StreetMaxLength);
    RuleFor(x => x.PostalCode).NotEmpty().MaximumLength(EmployeeConstraints.PostalCodeMaxLength);
    RuleFor(x => x.City).NotEmpty().MaximumLength(EmployeeConstraints.CityMaxLength);
    RuleFor(x => x.RowVersion).NotEmpty();
  }
}
