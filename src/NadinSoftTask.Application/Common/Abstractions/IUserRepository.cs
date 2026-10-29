using NadinSoftTask.Application.Common.Dtos;
using NadinSoftTask.Domain.Common;

namespace NadinSoftTask.Application.Common.Abstractions;

public interface IUserRepository
{
    Task<Result<UserDto>> CreateAsync(string username, string password);

    Task<UserDto?> GetByUsernameAsync(string username);

    Task<UserDto?> GetByIdAsync(string userId);

    Task<bool> CheckPasswordAsync(string userId, string password);
}
