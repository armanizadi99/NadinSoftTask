using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using NadinSoftTask.Application.Common.Abstractions;
using NadinSoftTask.Application.Common.Dtos;
using NadinSoftTask.Domain.Common;

namespace NadinSoftTask.Infrastructure.Identity;

public sealed class UserRepository(UserManager<ApplicationUser> userManager) : IUserRepository
{
    public async Task<Result<UserDto>> CreateAsync(
        string username,
        string password,
        CancellationToken cancellationToken = default)
    {
        ApplicationUser user = new(username);

        IdentityResult result;

        try
        {
            result = await userManager.CreateAsync(user, password);
        }
        catch (DbUpdateException)
        {
            return DuplicateUsername();
        }

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

    public async Task<UserDto?> GetByUsernameAsync(
        string username,
        CancellationToken cancellationToken = default)
    {
        ApplicationUser? user = await userManager.FindByNameAsync(username);

        return user is null
            ? null
            : ToDto(user);
    }

    public async Task<UserDto?> GetByIdAsync(
        string userId,
        CancellationToken cancellationToken = default)
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

    public async Task<bool> CheckPasswordAsync(
        string userId,
        string password,
        CancellationToken cancellationToken = default)
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
        new("Username is already taken.", DomainErrorKind.Conflict);
}
