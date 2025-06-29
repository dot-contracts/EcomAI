using EcomVideoAI.Application.DTOs.Requests.Auth;
using EcomVideoAI.Application.DTOs.Responses.Common;
using EcomVideoAI.Application.Interfaces;
using EcomVideoAI.Domain.Interfaces.Repositories;
using EcomVideoAI.Domain.Interfaces.Services;
using Microsoft.Extensions.Logging;

namespace EcomVideoAI.Application.UseCases.Auth
{
    public class LogoutUseCase : IAsyncUseCase<LogoutRequest, ApiResponse>
    {
        private readonly IRefreshTokenRepository _refreshTokenRepository;
        private readonly IJwtService _jwtService;
        private readonly ILogger<LogoutUseCase> _logger;

        public LogoutUseCase(
            IRefreshTokenRepository refreshTokenRepository,
            IJwtService jwtService,
            ILogger<LogoutUseCase> logger)
        {
            _refreshTokenRepository = refreshTokenRepository;
            _jwtService = jwtService;
            _logger = logger;
        }

        public async Task<ApiResponse> ExecuteAsync(LogoutRequest request, CancellationToken cancellationToken = default)
        {
            try
            {
                // Find and revoke the refresh token
                var refreshToken = await _refreshTokenRepository.GetByTokenAsync(request.RefreshToken, cancellationToken);
                
                if (refreshToken != null && !refreshToken.IsRevoked)
                {
                    refreshToken.Revoke();
                    await _refreshTokenRepository.SaveChangesAsync(cancellationToken);
                    
                    _logger.LogInformation("User logged out successfully. UserId: {UserId}", refreshToken.UserId);
                }
                else
                {
                    _logger.LogWarning("Logout attempt with invalid or already revoked refresh token");
                }

                // Note: For stateless JWT, we can't really "invalidate" access tokens on the server side
                // The client should delete the access token from storage
                // In a production environment, you might want to maintain a blacklist of revoked tokens
                
                return ApiResponse.SuccessResult("Logout successful");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred during logout");
                return ApiResponse.ErrorResult("An error occurred during logout. Please try again.");
            }
        }
    }
} 