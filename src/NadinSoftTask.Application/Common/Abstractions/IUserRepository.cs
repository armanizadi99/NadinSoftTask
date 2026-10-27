using NadinSoftTask.Application.Common.Dtos;
using NadinSoftTask.Domain.Common;

namespace NadinSoftTask.Application.Common.Abstractions;

public interface IUserRepository
{
    Task<Result<UserDto>> CreateAsync(
        string username,
        string password,
        CancellationToken cancellationToken = default);

    Task<UserDto?> GetByUsernameAsync(
        string username,
        CancellationToken cancellationToken = default);

    Task<UserDto?> GetByIdAsync(
        string userId,
        CancellationToken cancellationToken = default);

    Task<bool> CheckPasswordAsync(
        string userId,
        string password,
        CancellationToken cancellationToken = default);
}
