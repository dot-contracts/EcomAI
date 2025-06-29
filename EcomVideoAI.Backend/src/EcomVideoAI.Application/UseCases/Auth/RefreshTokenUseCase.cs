using EcomVideoAI.Application.DTOs.Requests.Auth;
using EcomVideoAI.Application.DTOs.Responses.Auth;
using EcomVideoAI.Application.DTOs.Responses.Common;
using EcomVideoAI.Application.Interfaces;
using EcomVideoAI.Domain.Interfaces.Repositories;
using EcomVideoAI.Domain.Interfaces.Services;
using Microsoft.Extensions.Logging;

namespace EcomVideoAI.Application.UseCases.Auth
{
    public class RefreshTokenUseCase : IAsyncUseCase<RefreshTokenRequest, ApiResponse<AuthResponse>>
    {
        private readonly IRefreshTokenRepository _refreshTokenRepository;
        private readonly IUserRepository _userRepository;
        private readonly IJwtService _jwtService;
        private readonly ILogger<RefreshTokenUseCase> _logger;

        public RefreshTokenUseCase(
            IRefreshTokenRepository refreshTokenRepository,
            IUserRepository userRepository,
            IJwtService jwtService,
            ILogger<RefreshTokenUseCase> logger)
        {
            _refreshTokenRepository = refreshTokenRepository;
            _userRepository = userRepository;
            _jwtService = jwtService;
            _logger = logger;
        }

        public async Task<ApiResponse<AuthResponse>> ExecuteAsync(RefreshTokenRequest request, CancellationToken cancellationToken = default)
        {
            try
            {
                // Validate the refresh token
                var refreshToken = await _refreshTokenRepository.GetByTokenAsync(request.RefreshToken, cancellationToken);
                
                if (refreshToken == null)
                {
                    _logger.LogWarning("Invalid refresh token provided");
                    return ApiResponse<AuthResponse>.ErrorResult("Invalid refresh token");
                }

                if (!refreshToken.IsActive)
                {
                    _logger.LogWarning("Refresh token is not active. UserId: {UserId}, Token: {Token}", 
                        refreshToken.UserId, refreshToken.Token.Substring(0, 10) + "...");
                    return ApiResponse<AuthResponse>.ErrorResult("Refresh token is expired or revoked");
                }

                // Get user with roles
                var user = await _userRepository.GetWithRolesAsync(refreshToken.UserId, cancellationToken);
                if (user == null)
                {
                    _logger.LogWarning("User not found for refresh token. UserId: {UserId}", refreshToken.UserId);
                    return ApiResponse<AuthResponse>.ErrorResult("User not found");
                }

                if (!user.IsActive)
                {
                    _logger.LogWarning("User account is deactivated. UserId: {UserId}", user.Id);
                    return ApiResponse<AuthResponse>.ErrorResult("User account is deactivated");
                }

                // Revoke the old refresh token
                refreshToken.Revoke();

                // Generate new tokens
                var roles = user.Roles.Select(r => r.RoleName).ToList();
                var newAccessToken = _jwtService.GenerateAccessToken(user, roles);
                var newRefreshToken = _jwtService.GenerateRefreshToken();

                // Create new refresh token entity
                var newRefreshTokenEntity = new Domain.Entities.RefreshToken(
                    user.Id,
                    newRefreshToken,
                    DateTime.UtcNow.AddDays(30)); // 30 days expiry

                user.RefreshTokens.Add(newRefreshTokenEntity);
                await _userRepository.SaveChangesAsync(cancellationToken);

                var userDto = new UserDto
                {
                    Id = user.Id,
                    Email = user.Email,
                    EmailVerified = user.EmailVerified,
                    FirstName = user.FirstName,
                    LastName = user.LastName,
                    AvatarUrl = user.AvatarUrl,
                    PhoneNumber = user.PhoneNumber,
                    DateOfBirth = user.DateOfBirth,
                    Timezone = user.Timezone,
                    Locale = user.Locale,
                    IsActive = user.IsActive,
                    LastLoginAt = user.LastLoginAt,
                    CreatedAt = user.CreatedAt,
                    Roles = roles
                };

                var authResponse = new AuthResponse
                {
                    AccessToken = newAccessToken,
                    RefreshToken = newRefreshToken,
                    User = userDto,
                    ExpiresAt = _jwtService.GetTokenExpiration(newAccessToken)
                };

                _logger.LogInformation("Token refreshed successfully for user: {UserId}", user.Id);

                return ApiResponse<AuthResponse>.SuccessResult(authResponse, "Token refreshed successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred during token refresh");
                return ApiResponse<AuthResponse>.ErrorResult("An error occurred during token refresh. Please try again.");
            }
        }
    }
} 