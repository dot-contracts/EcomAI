using EcomVideoAI.Application.DTOs.Requests.Auth;
using EcomVideoAI.Application.DTOs.Responses.Common;
using EcomVideoAI.Application.Interfaces;
using EcomVideoAI.Domain.Interfaces.Repositories;
using EcomVideoAI.Domain.Interfaces.Services;
using Microsoft.Extensions.Logging;

namespace EcomVideoAI.Application.UseCases.Auth
{
    public class ForgotPasswordUseCase : IAsyncUseCase<ForgotPasswordRequest, ApiResponse>
    {
        private readonly IUserRepository _userRepository;
        private readonly IPasswordService _passwordService;
        private readonly IEmailService _emailService;
        private readonly ILogger<ForgotPasswordUseCase> _logger;

        public ForgotPasswordUseCase(
            IUserRepository userRepository,
            IPasswordService passwordService,
            IEmailService emailService,
            ILogger<ForgotPasswordUseCase> logger)
        {
            _userRepository = userRepository;
            _passwordService = passwordService;
            _emailService = emailService;
            _logger = logger;
        }

        public async Task<ApiResponse> ExecuteAsync(ForgotPasswordRequest request, CancellationToken cancellationToken = default)
        {
            try
            {
                // Find user by email
                var user = await _userRepository.GetByEmailAsync(request.Email, cancellationToken);
                
                if (user == null)
                {
                    // For security reasons, don't reveal whether the email exists or not
                    _logger.LogWarning("Password reset requested for non-existent email: {Email}", request.Email);
                    return ApiResponse.SuccessResult("If the email address exists in our system, you will receive a password reset link shortly.");
                }

                if (!user.IsActive)
                {
                    _logger.LogWarning("Password reset requested for deactivated account: {Email}", request.Email);
                    return ApiResponse.ErrorResult("This account has been deactivated. Please contact support for assistance.");
                }

                // Generate secure reset token
                var resetToken = _passwordService.GenerateSecureToken(32);
                
                // In a production app, you would want to:
                // 1. Store the reset token in the database with an expiration time
                // 2. Associate it with the user
                // For now, we'll just send the email (this is a simplified implementation)
                
                // TODO: Store reset token in database with expiration
                // var passwordReset = new PasswordReset(user.Id, resetToken, DateTime.UtcNow.AddHours(1));
                // await _passwordResetRepository.AddAsync(passwordReset, cancellationToken);

                // Send password reset email
                await _emailService.SendPasswordResetEmailAsync(user.Email, user.FirstName ?? "User", resetToken);

                _logger.LogInformation("Password reset email sent to: {Email}", user.Email);

                return ApiResponse.SuccessResult("If the email address exists in our system, you will receive a password reset link shortly.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred during password reset request for email: {Email}", request.Email);
                return ApiResponse.ErrorResult("An error occurred while processing your request. Please try again.");
            }
        }
    }
} 