using MediatR;
using NadinSoftTask.Application.Common.Dtos;
using NadinSoftTask.Domain.Common;

namespace NadinSoftTask.Application.Commands.RefreshTokens;

public sealed record RefreshTokenCommand(
    string RefreshToken) : IRequest<Result<AuthResult>>;
