using System.Security.Claims;
using ContactManagement.Core.Interfaces;

namespace ContactManagement.API.Extensions;

public sealed class CurrentUserService : ICurrentUserService
{
    private readonly IHttpContextAccessor _accessor;

    public CurrentUserService(IHttpContextAccessor accessor) => _accessor = accessor;

    public int UserId => _accessor.HttpContext?.User.GetUserId() ?? 0;

    public bool IsAuthenticated => _accessor.HttpContext?.User.Identity?.IsAuthenticated ?? false;

    public string? Email => _accessor.HttpContext?.User.FindFirstValue(ClaimTypes.Email);
}
