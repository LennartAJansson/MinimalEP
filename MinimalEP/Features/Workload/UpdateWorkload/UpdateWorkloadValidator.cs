namespace MinimalEP.Features.Workload.UpdateWorkload;

using FluentValidation;

using MinimalEP.Domain.Model;

public class UpdateWorkloadValidator : AbstractValidator<UpdateWorkloadRequest>
{
  public UpdateWorkloadValidator()
  {
    RuleFor(x => x.Start).NotEmpty().WithMessage("Start is required.");
    RuleFor(x => x.Comments).MaximumLength(WorkloadConstraints.CommentsMaxLength);
    RuleFor(x => x.RowVersion).NotEmpty();
  }
}
