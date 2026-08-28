namespace MinimalEP.Features.Admin.AssignRole;

public record AssignRoleResponse(Guid UserId, IReadOnlyList<string> Roles);
