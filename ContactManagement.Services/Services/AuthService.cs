using ContactManagement.Core.DTOs.Auth;
using ContactManagement.Core.DTOs.Common;
using ContactManagement.Core.Entities;
using ContactManagement.Core.Exceptions;
using ContactManagement.Core.Interfaces;
using ContactManagement.Core.Interfaces.Services;
using ContactManagement.Services.Validators;
using Microsoft.Extensions.Logging;

namespace ContactManagement.Services.Services;

public sealed class AuthService : IAuthService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IJwtTokenGenerator _tokenGenerator;
    private readonly ILogger<AuthService> _logger;

    public AuthService(
        IUnitOfWork unitOfWork,
        IPasswordHasher passwordHasher,
        IJwtTokenGenerator tokenGenerator,
        ILogger<AuthService> logger)
    {
        _unitOfWork = unitOfWork;
        _passwordHasher = passwordHasher;
        _tokenGenerator = tokenGenerator;
        _logger = logger;
    }

    public async Task<ApiResponse<AuthResponse>> RegisterAsync(
        RegisterRequest request, CancellationToken cancellationToken = default)
    {
        ValidationService.Validate(request);

        var email = request.Email.Trim().ToLowerInvariant();
        if (await _unitOfWork.Users.ExistsByEmailAsync(email, cancellationToken))
        {
            throw new ConflictException("An account with this email already exists.");
        }

        var now = DateTime.UtcNow;
        var user = new User
        {
            FullName = request.FullName.Trim(),
            Email = email,
            PasswordHash = _passwordHasher.Hash(request.Password),
            CreatedAt = now,
            UpdatedAt = now
        };

        await _unitOfWork.Users.AddAsync(user, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        var token = _tokenGenerator.GenerateToken(user);
        _logger.LogInformation("User {UserId} registered successfully", user.UserId);

        return ApiResponse<AuthResponse>.Ok(
            new AuthResponse
            {
                UserId = user.UserId,
                FullName = user.FullName,
                Email = user.Email,
                Token = token.Token,
                ExpiresAt = token.ExpiresAt
            },
            "User registered successfully");
    }

    public async Task<ApiResponse<AuthResponse>> LoginAsync(
        LoginRequest request, CancellationToken cancellationToken = default)
    {
        ValidationService.Validate(request);

        var email = request.Email.Trim().ToLowerInvariant();
        var user = await _unitOfWork.Users.GetByEmailAsync(email, cancellationToken);
        if (user is null || !_passwordHasher.Verify(request.Password, user.PasswordHash))
        {
            _logger.LogWarning("Failed login attempt for email {Email}", email);
            throw new UnauthorizedException("Invalid email or password.");
        }

        var token = _tokenGenerator.GenerateToken(user);
        _logger.LogInformation("User {UserId} logged in successfully", user.UserId);

        return ApiResponse<AuthResponse>.Ok(
            new AuthResponse
            {
                UserId = user.UserId,
                FullName = user.FullName,
                Email = user.Email,
                Token = token.Token,
                ExpiresAt = token.ExpiresAt
            },
            "Login successful");
    }
}
