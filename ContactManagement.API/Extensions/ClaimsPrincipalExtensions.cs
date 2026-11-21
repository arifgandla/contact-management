using System.Security.Claims;
using ContactManagement.Core.Exceptions;

namespace ContactManagement.API.Extensions;

public static class ClaimsPrincipalExtensions
{
    public static int GetUserId(this ClaimsPrincipal principal)
    {
        var value = principal.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrWhiteSpace(value) || !int.TryParse(value, out var userId))
        {
            throw new UnauthorizedException("Invalid authentication token.");
        }

        return userId;
    }
}
