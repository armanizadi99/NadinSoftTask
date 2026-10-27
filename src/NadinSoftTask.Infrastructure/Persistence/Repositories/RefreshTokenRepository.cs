using Microsoft.EntityFrameworkCore;
using NadinSoftTask.Application.Common.Abstractions;
using NadinSoftTask.Domain.Users;

namespace NadinSoftTask.Infrastructure.Persistence.Repositories;

public sealed class RefreshTokenRepository(AppDbContext dbContext) : IRefreshTokenRepository
{
    public async Task AddAsync(
        RefreshToken refreshToken,
        CancellationToken cancellationToken = default) =>
        await dbContext.RefreshTokens.AddAsync(refreshToken, cancellationToken);

    public Task<RefreshToken?> GetByTokenAsync(
        string token,
        CancellationToken cancellationToken = default)
    {
        byte[] tokenHash = RefreshToken.HashToken(token);

        return dbContext.RefreshTokens
            .FirstOrDefaultAsync(
                refreshToken => refreshToken.TokenHash == tokenHash,
                cancellationToken);
    }
}
