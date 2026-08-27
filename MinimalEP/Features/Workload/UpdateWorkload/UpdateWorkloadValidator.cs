namespace MinimalEP.Features.Workload.UpdateWorkload;

using FluentValidation;

public class UpdateWorkloadValidator : AbstractValidator<UpdateWorkloadRequest>
{
  public UpdateWorkloadValidator()
  {
    RuleFor(x => x.Start).NotEmpty().WithMessage("Start is required.");
    RuleFor(x => x.Stop).GreaterThan(x => x.Start).When(x => x.Stop.HasValue)
        .WithMessage("Stop must be after Start.");
  }
}
