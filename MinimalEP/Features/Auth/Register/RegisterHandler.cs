namespace MinimalEP.Features.Auth.Register;

using Microsoft.AspNetCore.Identity;

using MinimalEP.Domain.Model;
using MinimalEP.Features.Core;

public class RegisterHandler(
  UserManager<ApplicationUser> userManager,
  IEmployeeRepository employeeRepository)
  : IRequestHandler<RegisterRequest, Result<RegisterResponse>>
{
  public async Task<Result<RegisterResponse>> HandleAsync(RegisterRequest request, CancellationToken cancellationToken)
  {
    var existing = await userManager.FindByEmailAsync(request.Email);
    if (existing is not null)
      return new Result<RegisterResponse>.Conflict($"A user with email '{request.Email}' already exists.");

    // Användarens Id och Employee-postens Id är samma —
    // det är detta som kopplar ihop Identity med Employee i interceptorn och Workload.
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

    // Skapa Employee-posten med samma Id som användaren
    var employee = new Employee
    {
      Id = userId,
      Name = request.Name,
      Age = request.Age,
      Position = request.Position,
      CreatedBy = userId  // Inget JWT finns vid registrering — sätt explicit
    };

    await employeeRepository.AddAsync(employee, cancellationToken);
    await employeeRepository.SaveChangesAsync(cancellationToken);

    return new Result<RegisterResponse>.Ok(new RegisterResponse(userId, user.Email!, employee.Name, employee.Position));
  }
}
