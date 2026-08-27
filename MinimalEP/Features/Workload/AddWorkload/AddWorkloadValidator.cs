namespace MinimalEP.Features.Workload.AddWorkload;

using FluentValidation;

public class AddWorkloadValidator : AbstractValidator<AddWorkloadRequest>
{
  public AddWorkloadValidator()
  {
    RuleFor(x => x.CustomerId).NotEmpty().WithMessage("CustomerId is required.");
    RuleFor(x => x.Start).NotEmpty().WithMessage("Start is required.");
    RuleFor(x => x.Stop).GreaterThan(x => x.Start).When(x => x.Stop.HasValue)
        .WithMessage("Stop must be after Start.");
  }
}
