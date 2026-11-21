namespace ContactManagement.Core.DTOs.Auth;

public sealed record TokenResult(string Token, DateTime ExpiresAt);
