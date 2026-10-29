using Microsoft.EntityFrameworkCore;
using NadinSoftTask.Infrastructure.Persistence;

namespace NadinSoftTask.Infrastructure.Identity;

public sealed class RefreshTokenStore(AppDbContext dbContext) : IRefreshTokenStore
{
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

    public async Task AddAsync(
        RefreshToken refreshToken,
        CancellationToken cancellationToken = default) =>
        await dbContext.RefreshTokens.AddAsync(refreshToken, cancellationToken);
}
