namespace NadinSoftTask.Api.Contracts.Auth;

/// <summary>
/// Credentials used to create a new user account.
/// </summary>
public sealed record RegisterRequest
{
    /// <summary>
    /// The username for the new account. Must be 3-50 characters and contain only
    /// letters, numbers, and underscores.
    /// </summary>
    public required string Username { get; init; }

    /// <summary>
    /// The password for the new account. Must be 6-100 characters.
    /// </summary>
    public required string Password { get; init; }
}
