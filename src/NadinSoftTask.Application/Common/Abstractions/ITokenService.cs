namespace NadinSoftTask.Application.Common.Abstractions;

public interface ITokenService
{
    (string AccessToken, DateTime ExpiresAt) GenerateAccessToken(string userId, string username);

    string GenerateRefreshToken();

    DateTime GetRefreshTokenExpiration();
}
