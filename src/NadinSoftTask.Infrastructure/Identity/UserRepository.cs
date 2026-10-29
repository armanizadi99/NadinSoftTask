using Microsoft.AspNetCore.Identity;
using NadinSoftTask.Application.Common;
using NadinSoftTask.Application.Common.Abstractions;
using NadinSoftTask.Application.Common.Dtos;
using NadinSoftTask.Domain.Common;

namespace NadinSoftTask.Infrastructure.Identity;

public sealed class UserRepository(UserManager<ApplicationUser> userManager) : IUserRepository
{
    public async Task<Result<UserDto>> CreateAsync(string username, string password)
    {
        if (await userManager.FindByNameAsync(username) is not null)
        {
            return DuplicateUsername();
        }

        ApplicationUser user = new(username);

        IdentityResult result = await userManager.CreateAsync(user, password);

        if (!result.Succeeded)
        {
            if (result.Errors.Any(error => error.Code == nameof(IdentityErrorDescriber.DuplicateUserName)))
            {
                return DuplicateUsername();
            }

            return new DomainError(
                string.Join("; ", result.Errors.Select(error => error.Description)),
                DomainErrorKind.Validation);
        }

        return new UserDto(user.Id.ToString(), user.UserName!);
    }

    public async Task<UserDto?> GetByUsernameAsync(string username)
    {
        ApplicationUser? user = await userManager.FindByNameAsync(username);

        return user is null
            ? null
            : ToDto(user);
    }

    public async Task<UserDto?> GetByIdAsync(string userId)
    {
        if (!Guid.TryParse(userId, out Guid id))
        {
            return null;
        }

        ApplicationUser? user = await userManager.FindByIdAsync(id.ToString());

        return user is null
            ? null
            : ToDto(user);
    }

    public async Task<bool> CheckPasswordAsync(string userId, string password)
    {
        if (!Guid.TryParse(userId, out Guid id))
        {
            return false;
        }

        ApplicationUser? user = await userManager.FindByIdAsync(id.ToString());

        return user is not null && await userManager.CheckPasswordAsync(user, password);
    }

    private static UserDto ToDto(ApplicationUser user) =>
        new(user.Id.ToString(), user.UserName!);

    private static DomainError DuplicateUsername() =>
        new(UserErrors.DuplicateUsername, DomainErrorKind.Conflict);
}
