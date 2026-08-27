namespace MinimalEP.Features.Auth.Register;

public record RegisterRequest(string Email, string Password, string Name, int Age, string Position);
