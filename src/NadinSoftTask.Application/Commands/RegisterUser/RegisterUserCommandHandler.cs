using MediatR;
using NadinSoftTask.Application.Common.Abstractions;
using NadinSoftTask.Application.Common.Dtos;
using NadinSoftTask.Domain.Common;
using NadinSoftTask.Domain.Users;

namespace NadinSoftTask.Application.Commands.RegisterUser;

public sealed class RegisterUserCommandHandler(
    IUserRepository userRepository,
    ITokenService tokenService,
    IRefreshTokenRepository refreshTokenRepository,
    IUnitOfWork unitOfWork) : IRequestHandler<RegisterUserCommand, Result<AuthResult>>
{
    public async Task<Result<AuthResult>> Handle(
        RegisterUserCommand request,
        CancellationToken cancellationToken)
    {
        Result<UserDto> createResult = await userRepository.CreateAsync(
            request.Username,
            request.Password,
            cancellationToken);

        if (createResult.IsFailure)
        {
            return createResult.Error;
        }

        UserDto user = createResult.Value!;

        (string accessToken, DateTime accessTokenExpiresAt) =
            tokenService.GenerateAccessToken(user.Id, user.Username);

        string refreshTokenValue = tokenService.GenerateRefreshToken();
        DateTime refreshTokenExpiresAt = tokenService.GetRefreshTokenExpiration();
        RefreshToken refreshToken = new(refreshTokenValue, user.Id, refreshTokenExpiresAt);

        await refreshTokenRepository.AddAsync(refreshToken, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return new AuthResult(
            accessToken,
            refreshTokenValue,
            accessTokenExpiresAt,
            refreshTokenExpiresAt,
            user.Id,
            user.Username);
    }
}
