#pragma warning disable CS9113 // Dependencies are read once the handler is implemented.

using MediatR;
using NadinSoftTask.Application.Common.Abstractions;
using NadinSoftTask.Application.Common.Dtos;
using NadinSoftTask.Domain.Common;

namespace NadinSoftTask.Application.Commands.RegisterUser;

public sealed class RegisterUserCommandHandler(
    IUserRepository userRepository,
    ITokenService tokenService,
    IRefreshTokenRepository refreshTokenRepository,
    IUnitOfWork unitOfWork) : IRequestHandler<RegisterUserCommand, Result<AuthResult>>
{
    public Task<Result<AuthResult>> Handle(
        RegisterUserCommand request,
        CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }
}
