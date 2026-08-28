namespace MinimalEP.Features.Admin.AssignRole;

using Microsoft.AspNetCore.Identity;

using MinimalEP.Domain.Core;
using MinimalEP.Domain.Model;
using MinimalEP.Features.Core;

public class AssignRoleHandler(
  UserManager<ApplicationUser> userManager,
  IUserContext userContext)
  : IRequestHandler<AssignRoleRequest, Result<AssignRoleResponse>>
{
  public async Task<Result<AssignRoleResponse>> HandleAsync(AssignRoleRequest request, CancellationToken cancellationToken)
  {
    var user = await userManager.FindByIdAsync(request.UserId.ToString());
    if (user is null)
      return new Result<AssignRoleResponse>.NotFound();

    var currentRoles = await userManager.GetRolesAsync(user);
    var callerIsSuperAdmin = userContext.IsInRole(Roles.SuperAdmin);

    // Business rule: a plain Admin may manage everyone except other Admins/SuperAdmins,
    // and may not grant the Admin/SuperAdmin role themselves. Only SuperAdmin can do that.
    // This is resource-based authorization — the [Authorize] policy only gets you in the door
    // ("AdminOrAbove"), this check decides what you may do to *this specific* target.
    if (!callerIsSuperAdmin)
    {
      if (currentRoles.Contains(Roles.SuperAdmin) || currentRoles.Contains(Roles.Admin))
        return new Result<AssignRoleResponse>.Conflict("Only a SuperAdmin may modify another Admin's or SuperAdmin's role.");

      if (request.Role is Roles.SuperAdmin or Roles.Admin)
        return new Result<AssignRoleResponse>.Conflict("Only a SuperAdmin may grant the Admin or SuperAdmin role.");
    }

    if (currentRoles.Count > 0)
      await userManager.RemoveFromRolesAsync(user, currentRoles);

    await userManager.AddToRoleAsync(user, request.Role);

    return new Result<AssignRoleResponse>.Ok(new AssignRoleResponse(user.Id, [request.Role]));
  }
}
