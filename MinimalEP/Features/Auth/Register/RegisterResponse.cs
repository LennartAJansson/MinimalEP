namespace MinimalEP.Features.Auth.Register;

public record RegisterResponse(Guid UserId, string Email, string Name, string Position);
