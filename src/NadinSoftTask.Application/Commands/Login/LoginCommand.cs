using MediatR;
using NadinSoftTask.Application.Common.Abstractions;
using NadinSoftTask.Application.Common.Dtos;
using NadinSoftTask.Domain.Common;

namespace NadinSoftTask.Application.Commands.Login;

public sealed record LoginCommand(
    string Username,
    string Password) : IRequest<Result<AuthResult>>, IUserCredentialsCommand;
