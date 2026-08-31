namespace MinimalEP.Features.Admin.CreateEmployeeAccount;

using System.Security.Cryptography;

using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

using MinimalEP.Domain.Core;
using MinimalEP.Domain.Model;
using MinimalEP.Features.Core;
using MinimalEP.Infrastructure.Auth;
using MinimalEP.Infrastructure.Data.Context;

public class CreateEmployeeAccountHandler(
  UserManager<ApplicationUser> userManager,
  IEmployeeRepository employeeRepository,
  IUserContext userContext,
  ApplicationDbContext context)
  : IRequestHandler<CreateEmployeeAccountRequest, Result<CreateEmployeeAccountResponse>>
{
  public async Task<Result<CreateEmployeeAccountResponse>> HandleAsync(CreateEmployeeAccountRequest request, CancellationToken cancellationToken)
  {
    // Same resource-based rule as AssignRole: a plain Admin cannot create another Admin/SuperAdmin account.
    if (request.Role is Roles.SuperAdmin or Roles.Admin && !userContext.IsInRole(Roles.SuperAdmin))
      return new Result<CreateEmployeeAccountResponse>.Conflict("Only a SuperAdmin may create an Admin or SuperAdmin account.");

    var existing = await userManager.FindByEmailAsync(request.Email);
    if (existing is not null)
      return new Result<CreateEmployeeAccountResponse>.Conflict($"A user with email '{request.Email}' already exists.");

    await using var transaction = await context.Database.BeginTransactionAsync(cancellationToken);

    var userId = Guid.CreateVersion7();

    var user = new ApplicationUser
    {
      Id = userId,
      UserName = request.Email,
      Email = request.Email
    };

    // Admin never chooses the user's password — a random temporary one is generated and
    // discarded immediately. The user must set their own via the returned reset token.
    var temporaryPassword = GenerateTemporaryPassword();
    var result = await userManager.CreateAsync(user, temporaryPassword);
    if (!result.Succeeded)
    {
      var errors = string.Join(", ", result.Errors.Select(e => e.Description));
      return new Result<CreateEmployeeAccountResponse>.Conflict(errors);
    }

    var roleResult = await userManager.AddToRoleAsync(user, request.Role);
    if (!roleResult.Succeeded)
    {
      var errors = string.Join(", ", roleResult.Errors.Select(e => e.Description));
      return new Result<CreateEmployeeAccountResponse>.Conflict(errors);
    }

    var employee = new Employee
    {
      Id = userId,
      Email = user.Email!,
      GivenName = request.GivenName,
      Surname = request.Surname,
      Age = request.Age,
      Position = request.Position,
      PhoneNumber = request.PhoneNumber,
      Address = new Address
      {
        Street = request.Street,
        PostalCode = request.PostalCode,
        City = request.City
      },
      CreatedBy = userContext.UserId
    };

    await employeeRepository.AddAsync(employee, cancellationToken);
    await employeeRepository.SaveChangesAsync(cancellationToken);
    await transaction.CommitAsync(cancellationToken);

    var resetToken = await userManager.GeneratePasswordResetTokenAsync(user);

    return new Result<CreateEmployeeAccountResponse>.Ok(new CreateEmployeeAccountResponse(
      userId, user.Email!, employee.Name, employee.Position, request.Role, resetToken));
  }

  private static string GenerateTemporaryPassword()
  {
    // Satisfies the configured password policy (digit + length >= 8) without ever being used —
    // the user resets it via the token before first login.
    return $"Tmp{Convert.ToBase64String(RandomNumberGenerator.GetBytes(AuthDefaults.TemporaryPasswordRandomBytes))}1!";
  }
}
