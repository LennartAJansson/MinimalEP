namespace MinimalEP.Features.Auth.Register;

using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

using MinimalEP.Domain.Core;
using MinimalEP.Domain.Model;
using MinimalEP.Features.Core;
using MinimalEP.Infrastructure.Data.Context;

public class RegisterHandler(
  UserManager<ApplicationUser> userManager,
  IEmployeeRepository employeeRepository,
  ApplicationDbContext context)
  : IRequestHandler<RegisterRequest, Result<RegisterResponse>>
{
  public async Task<Result<RegisterResponse>> HandleAsync(RegisterRequest request, CancellationToken cancellationToken)
  {
    var existing = await userManager.FindByEmailAsync(request.Email);
    if (existing is not null)
      return new Result<RegisterResponse>.Conflict($"A user with email '{request.Email}' already exists.");

    await using var transaction = await context.Database.BeginTransactionAsync(cancellationToken);

    // ApplicationUser and Employee intentionally share the same identity.
    var userId = Guid.CreateVersion7();

    var user = new ApplicationUser
    {
      Id = userId,
      UserName = request.Email,
      Email = request.Email
    };

    var result = await userManager.CreateAsync(user, request.Password);
    if (!result.Succeeded)
    {
      var errors = string.Join(", ", result.Errors.Select(e => e.Description));
      return new Result<RegisterResponse>.Conflict(errors);
    }

    var roleResult = await userManager.AddToRoleAsync(user, Roles.User);
    if (!roleResult.Succeeded)
    {
      var errors = string.Join(", ", roleResult.Errors.Select(e => e.Description));
      return new Result<RegisterResponse>.Conflict(errors);
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
      CreatedBy = userId  // Registration has no authenticated user context.
    };

    await employeeRepository.AddAsync(employee, cancellationToken);
    await employeeRepository.SaveChangesAsync(cancellationToken);
    await transaction.CommitAsync(cancellationToken);

    return new Result<RegisterResponse>.Ok(new RegisterResponse(userId, user.Email!, employee.Name, employee.Position));
  }
}

