namespace NadinSoftTask.Application.Common.Abstractions;

public interface IUserCredentialsCommand
{
    string Username { get; }

    string Password { get; }
}
