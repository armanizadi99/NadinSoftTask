using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using NadinSoftTask.Application.Common.Abstractions;

namespace NadinSoftTask.Api;

public sealed class CurrentUser(IHttpContextAccessor httpContextAccessor) : ICurrentUser
{
    public string UserId
    {
        get
        {
            ClaimsPrincipal? principal = httpContextAccessor.HttpContext?.User;

            Claim? userId = principal?.FindFirst(ClaimTypes.NameIdentifier)
                ?? principal?.FindFirst("sub");

            return userId?.Value ?? string.Empty;
        }
    }
}
