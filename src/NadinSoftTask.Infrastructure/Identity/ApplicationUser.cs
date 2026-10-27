using Microsoft.AspNetCore.Identity;

namespace NadinSoftTask.Infrastructure.Identity;

public sealed class ApplicationUser : IdentityUser<Guid>
{
    public ApplicationUser()
    {
        Id = Guid.NewGuid();
    }

    public ApplicationUser(string userName)
        : this()
    {
        UserName = userName;
    }
}
