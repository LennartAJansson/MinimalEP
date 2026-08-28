namespace MinimalEP.Features.Admin.AssignRole;

using FluentValidation;

using MinimalEP.Domain.Core;

public class AssignRoleValidator : AbstractValidator<AssignRoleRequest>
{
  public AssignRoleValidator()
  {
    RuleFor(x => x.UserId).NotEmpty();
    RuleFor(x => x.Role)
      .Must(role => Roles.All.Contains(role))
      .WithMessage($"Role must be one of: {string.Join(", ", Roles.All)}.");
  }
}
