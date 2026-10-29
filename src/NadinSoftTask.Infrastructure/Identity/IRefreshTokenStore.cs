namespace NadinSoftTask.Infrastructure.Identity;

public interface IRefreshTokenStore
{
    Task<RefreshToken?> GetByTokenAsync(
        string token,
        CancellationToken cancellationToken = default);

    Task AddAsync(
        RefreshToken refreshToken,
        CancellationToken cancellationToken = default);
}
