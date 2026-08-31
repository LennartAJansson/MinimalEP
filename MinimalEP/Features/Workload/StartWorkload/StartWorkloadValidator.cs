namespace MinimalEP.Features.Workload.StartWorkload;

using FluentValidation;

using MinimalEP.Domain.Model;

public class StartWorkloadValidator : AbstractValidator<StartWorkloadRequest>
{
  public StartWorkloadValidator()
  {
    RuleFor(x => x.CustomerId).NotEmpty().WithMessage("CustomerId is required.");
    RuleFor(x => x.Start).NotEmpty().WithMessage("Start is required.");
    RuleFor(x => x.Comments).MaximumLength(WorkloadConstraints.CommentsMaxLength);
  }
}
