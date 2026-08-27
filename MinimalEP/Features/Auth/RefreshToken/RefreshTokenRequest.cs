namespace MinimalEP.Features.Auth.RefreshToken;

public record RefreshTokenRequest(string AccessToken, string RefreshToken);
