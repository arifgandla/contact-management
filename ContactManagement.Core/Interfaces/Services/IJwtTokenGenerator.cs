using ContactManagement.Core.DTOs.Auth;
using ContactManagement.Core.Entities;

namespace ContactManagement.Core.Interfaces.Services;

public interface IJwtTokenGenerator
{
    TokenResult GenerateToken(User user);
}
