namespace NadinSoftTask.Application.Common;

public static class UserConstraints
{
    public const int MinUsernameLength = 3;

    public const int MaxUsernameLength = 50;

    public const int MinPasswordLength = 6;

    public const int MaxPasswordLength = 100;

    public const string UsernameAllowedCharactersPattern = "^[a-zA-Z0-9_]+$";
}
