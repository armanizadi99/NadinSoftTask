using MediatR;
using NadinSoftTask.Application.Common.Abstractions;
using NadinSoftTask.Application.Common.Dtos;
using NadinSoftTask.Domain.Common;

namespace NadinSoftTask.Application.Commands.RegisterUser;

public sealed class RegisterUserCommandHandler(
    IUserRepository userRepository,
    ITokenService tokenService) : IRequestHandler<RegisterUserCommand, Result<AuthResult>>
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

        return await tokenService.GenerateTokensAsync(
            user.Id,
            user.Username,
            cancellationToken);
    }
}
