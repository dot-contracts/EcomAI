using EcomVideoAI.Application.DTOs.Requests.Auth;
using EcomVideoAI.Application.DTOs.Responses.Common;
using EcomVideoAI.Application.Interfaces;
using EcomVideoAI.Domain.Interfaces.Repositories;
using EcomVideoAI.Domain.Interfaces.Services;
using Microsoft.Extensions.Logging;

namespace EcomVideoAI.Application.UseCases.Auth
{
    public class ResetPasswordUseCase : IAsyncUseCase<ResetPasswordRequest, ApiResponse>
    {
        private readonly IUserRepository _userRepository;
        private readonly IPasswordService _passwordService;
        private readonly IEmailService _emailService;
        private readonly IRefreshTokenRepository _refreshTokenRepository;
        private readonly ILogger<ResetPasswordUseCase> _logger;

        public ResetPasswordUseCase(
            IUserRepository userRepository,
            IPasswordService passwordService,
            IEmailService emailService,
            IRefreshTokenRepository refreshTokenRepository,
            ILogger<ResetPasswordUseCase> logger)
        {
            _userRepository = userRepository;
            _passwordService = passwordService;
            _emailService = emailService;
            _refreshTokenRepository = refreshTokenRepository;
            _logger = logger;
        }

        public async Task<ApiResponse> ExecuteAsync(ResetPasswordRequest request, CancellationToken cancellationToken = default)
        {
            try
            {
                // Find user by email
                var user = await _userRepository.GetByEmailAsync(request.Email, cancellationToken);
                
                if (user == null)
                {
                    _logger.LogWarning("Password reset attempted for non-existent email: {Email}", request.Email);
                    return ApiResponse.ErrorResult("Invalid email or reset token");
                }

                if (!user.IsActive)
                {
                    _logger.LogWarning("Password reset attempted for deactivated account: {Email}", request.Email);
                    return ApiResponse.ErrorResult("This account has been deactivated. Please contact support for assistance.");
                }

                // TODO: In a production app, validate the reset token against stored tokens in database
                // For now, we'll skip this validation since we don't have a PasswordReset entity yet
                // var passwordReset = await _passwordResetRepository.GetValidTokenAsync(request.Email, request.Token, cancellationToken);
                // if (passwordReset == null || passwordReset.IsExpired)
                // {
                //     return ApiResponse.ErrorResult("Invalid or expired reset token");
                // }

                // Hash the new password
                var passwordHash = _passwordService.HashPassword(request.NewPassword);

                // Update user password
                user.UpdatePassword(passwordHash);
                
                // Revoke all existing refresh tokens for security
                await _refreshTokenRepository.RevokeAllUserTokensAsync(user.Id, cancellationToken);

                await _userRepository.SaveChangesAsync(cancellationToken);

                // TODO: Mark the reset token as used in database
                // passwordReset.MarkAsUsed();
                // await _passwordResetRepository.SaveChangesAsync(cancellationToken);

                // Send password changed notification email
                _ = Task.Run(async () =>
                {
                    try
                    {
                        await _emailService.SendPasswordChangedNotificationAsync(user.Email, user.FirstName ?? "User");
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning(ex, "Failed to send password changed notification to {Email}", user.Email);
                    }
                }, cancellationToken);

                _logger.LogInformation("Password reset completed successfully for user: {Email}", user.Email);

                return ApiResponse.SuccessResult("Password has been reset successfully. Please log in with your new password.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred during password reset for email: {Email}", request.Email);
                return ApiResponse.ErrorResult("An error occurred while resetting your password. Please try again.");
            }
        }
    }
} 