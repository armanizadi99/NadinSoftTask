using NadinSoftTask.Application.Common.Dtos;
using NadinSoftTask.Domain.Users;

namespace NadinSoftTask.Tests.Application.TestData;

internal static class AuthTestData
{
    public const string Username = "arman";
    public const string Password = "secret123";
    public const string UserId = "user-123";
    public const string AccessToken = "access-token";
    public const string RefreshToken = "refresh-token";
    public const string NewRefreshToken = "new-refresh-token";

    public static readonly DateTime AccessTokenExpiresAt = new(2030, 1, 1, 0, 0, 0, DateTimeKind.Utc);
    public static readonly DateTime RefreshTokenExpiresAt = new(2030, 1, 1, 0, 5, 0, DateTimeKind.Utc);

    public static UserDto User => new(UserId, Username);

    public static RefreshToken CreateRefreshToken(
        string token = RefreshToken,
        string userId = UserId,
        DateTime? expiresAt = null) =>
        new(token, userId, expiresAt ?? DateTime.UtcNow.AddMinutes(5));
}
