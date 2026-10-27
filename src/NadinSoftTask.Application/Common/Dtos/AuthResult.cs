namespace NadinSoftTask.Application.Common.Dtos;

public sealed record AuthResult(
    string AccessToken,
    string RefreshToken,
    DateTime AccessTokenExpiresAt,
    DateTime RefreshTokenExpiresAt,
    string UserId,
    string Username);
