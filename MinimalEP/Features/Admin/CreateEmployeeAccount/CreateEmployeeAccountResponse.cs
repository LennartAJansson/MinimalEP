namespace MinimalEP.Features.Admin.CreateEmployeeAccount;

// PasswordResetToken is returned directly for demo purposes only — in a real system this would be
// emailed to the user via a "set your password" link, never returned in an API response.
public record CreateEmployeeAccountResponse(Guid UserId, string Email, string Name, string Position, string Role, string PasswordResetToken);
