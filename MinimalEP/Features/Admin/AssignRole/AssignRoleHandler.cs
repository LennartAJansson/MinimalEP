namespace MinimalEP.Features.Admin.AssignRole;

using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

using MinimalEP.Domain.Core;
using MinimalEP.Domain.Model;
using MinimalEP.Features.Core;
using MinimalEP.Infrastructure.Data.Context;

public class AssignRoleHandler(
  UserManager<ApplicationUser> userManager,
  IUserContext userContext,
  ApplicationDbContext context,
  ILogger<AssignRoleHandler> logger)
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

    if (currentRoles.Contains(request.Role))
      return new Result<AssignRoleResponse>.Ok(new AssignRoleResponse(user.Id, [request.Role]));

    if (userContext.UserId == user.Id && currentRoles.Contains(Roles.SuperAdmin) && request.Role != Roles.SuperAdmin)
      return new Result<AssignRoleResponse>.Conflict("A SuperAdmin may not remove their own SuperAdmin role.");

    if (currentRoles.Contains(Roles.SuperAdmin) && request.Role != Roles.SuperAdmin)
    {
      var superAdmins = await userManager.GetUsersInRoleAsync(Roles.SuperAdmin);
      if (superAdmins.Count == 1)
        return new Result<AssignRoleResponse>.Conflict("The last SuperAdmin may not be demoted.");
    }

    await using var transaction = await context.Database.BeginTransactionAsync(cancellationToken);

    if (currentRoles.Count > 0)
    {
      var removeResult = await userManager.RemoveFromRolesAsync(user, currentRoles);
      if (!removeResult.Succeeded)
      {
        var errors = string.Join(", ", removeResult.Errors.Select(e => e.Description));
        return new Result<AssignRoleResponse>.Conflict(errors);
      }
    }

    var addResult = await userManager.AddToRoleAsync(user, request.Role);
    if (!addResult.Succeeded)
    {
      var errors = string.Join(", ", addResult.Errors.Select(e => e.Description));
      return new Result<AssignRoleResponse>.Conflict(errors);
    }

    await transaction.CommitAsync(cancellationToken);
    logger.LogInformation(
      "User {TargetUserId} role changed to {Role} by {ActorUserId}.",
      user.Id,
      request.Role,
      userContext.UserId);

    return new Result<AssignRoleResponse>.Ok(new AssignRoleResponse(user.Id, [request.Role]));
  }
}
