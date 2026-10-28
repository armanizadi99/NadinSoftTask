using NadinSoftTask.Application.Common.Dtos;
using NadinSoftTask.Domain.Common;

namespace NadinSoftTask.Application.Common.Abstractions;

public interface ITokenService
{
    Task<Result<AuthResult>> GenerateTokensAsync(
        string userId,
        string userName,
        CancellationToken cancellationToken = default);

    Task<Result<AuthResult>> RefreshTokensAsync(
        string refreshToken,
        CancellationToken cancellationToken = default);
}
