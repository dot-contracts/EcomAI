using EcomVideoAI.Application.DTOs.Requests.Auth;
using EcomVideoAI.Application.DTOs.Responses.Auth;
using EcomVideoAI.Application.DTOs.Responses.Common;
using EcomVideoAI.Application.Interfaces;
using EcomVideoAI.Domain.Entities;
using EcomVideoAI.Domain.Interfaces.Repositories;
using EcomVideoAI.Domain.Interfaces.Services;
using Microsoft.Extensions.Logging;

namespace EcomVideoAI.Application.UseCases.Auth
{
    public class LoginUseCase : IAsyncUseCase<LoginRequest, ApiResponse<AuthResponse>>
    {
        private readonly IUserRepository _userRepository;
        private readonly IPasswordService _passwordService;
        private readonly IJwtService _jwtService;
        private readonly ILogger<LoginUseCase> _logger;

        public LoginUseCase(
            IUserRepository userRepository,
            IPasswordService passwordService,
            IJwtService jwtService,
            ILogger<LoginUseCase> logger)
        {
            _userRepository = userRepository;
            _passwordService = passwordService;
            _jwtService = jwtService;
            _logger = logger;
        }

        public async Task<ApiResponse<AuthResponse>> ExecuteAsync(LoginRequest request, CancellationToken cancellationToken = default)
        {
            try
            {
                // Get user by email
                var user = await _userRepository.GetByEmailAsync(request.Email.ToLowerInvariant(), cancellationToken);

                if (user == null)
                {
                    _logger.LogWarning("Login attempt with non-existent email: {Email}", request.Email);
                    return ApiResponse<AuthResponse>.ErrorResult("Invalid email or password");
                }

                // Check if account is locked
                if (user.IsLocked)
                {
                    _logger.LogWarning("Login attempt on locked account: {Email}", request.Email);
                    return ApiResponse<AuthResponse>.ErrorResult($"Account is locked until {user.LockedUntil:yyyy-MM-dd HH:mm} UTC");
                }

                // Check if account is active
                if (!user.IsActive)
                {
                    _logger.LogWarning("Login attempt on inactive account: {Email}", request.Email);
                    return ApiResponse<AuthResponse>.ErrorResult("Account is deactivated. Please contact support.");
                }

                // Verify password
                if (!_passwordService.VerifyPassword(request.Password, user.PasswordHash))
                {
                    // Record failed login attempt
                    user.RecordFailedLogin();
                    await _userRepository.SaveChangesAsync(cancellationToken);

                    _logger.LogWarning("Failed login attempt for email: {Email}. Attempt count: {Attempts}", 
                        request.Email, user.FailedLoginAttempts);

                    return ApiResponse<AuthResponse>.ErrorResult("Invalid email or password");
                }

                // Get user with roles for complete information
                var userWithRoles = await _userRepository.GetWithRolesAsync(user.Id, cancellationToken);
                if (userWithRoles == null)
                {
                    return ApiResponse<AuthResponse>.ErrorResult("User data could not be retrieved");
                }

                // Record successful login
                userWithRoles.RecordSuccessfulLogin();
                await _userRepository.SaveChangesAsync(cancellationToken);

                // Get user roles
                var roles = userWithRoles.Roles.Select(r => r.RoleName).ToList();

                // Generate tokens
                var accessToken = _jwtService.GenerateAccessToken(userWithRoles, roles);
                var refreshToken = _jwtService.GenerateRefreshToken();

                // Create refresh token with appropriate expiry based on RememberMe
                var refreshTokenExpiry = request.RememberMe 
                    ? DateTime.UtcNow.AddDays(30) 
                    : DateTime.UtcNow.AddDays(7);

                var refreshTokenEntity = new RefreshToken(
                    userWithRoles.Id,
                    refreshToken,
                    refreshTokenExpiry);

                userWithRoles.RefreshTokens.Add(refreshTokenEntity);
                await _userRepository.SaveChangesAsync(cancellationToken);

                var userDto = new UserDto
                {
                    Id = userWithRoles.Id,
                    Email = userWithRoles.Email,
                    EmailVerified = userWithRoles.EmailVerified,
                    FirstName = userWithRoles.FirstName,
                    LastName = userWithRoles.LastName,
                    AvatarUrl = userWithRoles.AvatarUrl,
                    PhoneNumber = userWithRoles.PhoneNumber,
                    DateOfBirth = userWithRoles.DateOfBirth,
                    Timezone = userWithRoles.Timezone,
                    Locale = userWithRoles.Locale,
                    IsActive = userWithRoles.IsActive,
                    LastLoginAt = userWithRoles.LastLoginAt,
                    CreatedAt = userWithRoles.CreatedAt,
                    Roles = roles
                };

                var authResponse = new AuthResponse
                {
                    AccessToken = accessToken,
                    RefreshToken = refreshToken,
                    User = userDto,
                    ExpiresAt = _jwtService.GetTokenExpiration(accessToken)
                };

                _logger.LogInformation("User logged in successfully: {Email}", userWithRoles.Email);

                return ApiResponse<AuthResponse>.SuccessResult(authResponse, "Login successful");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred during login for email: {Email}", request.Email);
                return ApiResponse<AuthResponse>.ErrorResult("An error occurred during login. Please try again.");
            }
        }
    }
} 