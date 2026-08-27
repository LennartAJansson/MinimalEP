namespace MinimalEP.Features.Auth.RefreshToken;

public record RefreshTokenResponse(string AccessToken, string RefreshToken, DateTimeOffset Expiry);
