namespace NadinSoftTask.Api.Contracts.Auth;

/// <summary>
/// Requests a new token pair using a valid refresh token.
/// </summary>
public sealed record RefreshTokenRequest
{
    /// <summary>
    /// The refresh token returned by register, login, or a previous refresh operation.
    /// </summary>
    public required string RefreshToken { get; init; }
}
