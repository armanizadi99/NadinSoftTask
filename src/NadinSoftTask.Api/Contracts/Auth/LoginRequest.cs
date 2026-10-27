namespace NadinSoftTask.Api.Contracts.Auth;

/// <summary>
/// Credentials used to authenticate an existing user.
/// </summary>
public sealed record LoginRequest
{
    /// <summary>
    /// The account username.
    /// </summary>
    public required string Username { get; init; }

    /// <summary>
    /// The account password.
    /// </summary>
    public required string Password { get; init; }
}
