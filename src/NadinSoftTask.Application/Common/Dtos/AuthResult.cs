namespace NadinSoftTask.Application.Common.Dtos;

/// <summary>
/// Authentication tokens issued after a successful register, login, or refresh operation.
/// </summary>
public sealed record AuthResult
{
    /// <summary>
    /// Creates an authentication result.
    /// </summary>
    /// <param name="accessToken">The signed JWT access token.</param>
    /// <param name="refreshToken">The opaque refresh token used to obtain a new access token.</param>
    /// <param name="accessTokenExpiresAt">The UTC date and time when the access token expires.</param>
    /// <param name="refreshTokenExpiresAt">The UTC date and time when the refresh token expires.</param>
    /// <param name="userId">The identifier of the authenticated user.</param>
    /// <param name="username">The username of the authenticated user.</param>
    public AuthResult(
        string accessToken,
        string refreshToken,
        DateTime accessTokenExpiresAt,
        DateTime refreshTokenExpiresAt,
        string userId,
        string username)
    {
        AccessToken = accessToken;
        RefreshToken = refreshToken;
        AccessTokenExpiresAt = accessTokenExpiresAt;
        RefreshTokenExpiresAt = refreshTokenExpiresAt;
        UserId = userId;
        Username = username;
    }

    /// <summary>
    /// The signed JWT access token. Send it as a Bearer token in the Authorization header.
    /// </summary>
    public string AccessToken { get; init; }

    /// <summary>
    /// The opaque refresh token. Send it to the refresh endpoint to obtain a new token pair.
    /// </summary>
    public string RefreshToken { get; init; }

    /// <summary>
    /// The UTC date and time when the access token expires.
    /// </summary>
    public DateTime AccessTokenExpiresAt { get; init; }

    /// <summary>
    /// The UTC date and time when the refresh token expires.
    /// </summary>
    public DateTime RefreshTokenExpiresAt { get; init; }

    /// <summary>
    /// The identifier of the authenticated user.
    /// </summary>
    public string UserId { get; init; }

    /// <summary>
    /// The username of the authenticated user.
    /// </summary>
    public string Username { get; init; }
}
