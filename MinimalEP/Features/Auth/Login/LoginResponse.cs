namespace MinimalEP.Features.Auth.Login;

public record LoginResponse(string AccessToken, string RefreshToken, DateTimeOffset Expiry);
