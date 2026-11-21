using ContactManagement.Core.DTOs.Auth;
using ContactManagement.Core.DTOs.Common;

namespace ContactManagement.Core.Interfaces.Services;

public interface IAuthService
{
    Task<ApiResponse<AuthResponse>> RegisterAsync(RegisterRequest request, CancellationToken cancellationToken = default);
    Task<ApiResponse<AuthResponse>> LoginAsync(LoginRequest request, CancellationToken cancellationToken = default);
}
