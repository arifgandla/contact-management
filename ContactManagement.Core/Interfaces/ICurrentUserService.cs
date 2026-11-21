namespace ContactManagement.Core.Interfaces;

public interface ICurrentUserService
{
    int UserId { get; }
    bool IsAuthenticated { get; }
    string? Email { get; }
}
