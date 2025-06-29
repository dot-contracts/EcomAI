using EcomVideoAI.Application.DTOs.Requests.Auth;
using EcomVideoAI.Application.DTOs.Responses.Common;
using EcomVideoAI.Application.Interfaces;
using EcomVideoAI.Domain.Entities;
using EcomVideoAI.Domain.Interfaces.Repositories;
using EcomVideoAI.Domain.Interfaces.Services;
using Microsoft.Extensions.Logging;

namespace EcomVideoAI.Application.UseCases.Auth
{
    public class ResendVerificationUseCase : IAsyncUseCase<ResendVerificationRequest, ApiResponse>
    {
        private readonly IUserRepository _userRepository;
        private readonly IEmailVerificationTokenRepository _emailVerificationTokenRepository;
        private readonly IPasswordService _passwordService;
        private readonly IEmailService _emailService;
        private readonly ILogger<ResendVerificationUseCase> _logger;

        public ResendVerificationUseCase(
            IUserRepository userRepository,
            IEmailVerificationTokenRepository emailVerificationTokenRepository,
            IPasswordService passwordService,
            IEmailService emailService,
            ILogger<ResendVerificationUseCase> logger)
        {
            _userRepository = userRepository;
            _emailVerificationTokenRepository = emailVerificationTokenRepository;
            _passwordService = passwordService;
            _emailService = emailService;
            _logger = logger;
        }

        public async Task<ApiResponse> ExecuteAsync(ResendVerificationRequest request, CancellationToken cancellationToken = default)
        {
            try
            {
                // Find user by email
                var user = await _userRepository.GetByEmailAsync(request.Email, cancellationToken);
                
                if (user == null)
                {
                    // For security reasons, don't reveal whether the email exists or not
                    _logger.LogWarning("Verification email resend requested for non-existent email: {Email}", request.Email);
                    return ApiResponse.SuccessResult("If the email address exists in our system, you will receive a verification email shortly.");
                }

                if (user.EmailVerified)
                {
                    _logger.LogInformation("Verification email resend requested for already verified email: {Email}", request.Email);
                    return ApiResponse.SuccessResult("This email address has already been verified.");
                }

                if (!user.IsActive)
                {
                    _logger.LogWarning("Verification email resend requested for deactivated account: {Email}", request.Email);
                    return ApiResponse.ErrorResult("This account has been deactivated. Please contact support for assistance.");
                }

                // Revoke any existing verification tokens for this user
                await _emailVerificationTokenRepository.RevokeAllUserTokensAsync(user.Id, cancellationToken);

                // Generate new verification token
                var verificationToken = _passwordService.GenerateSecureToken(32);
                
                // Create and store the verification token (expires in 24 hours)
                var emailVerificationToken = new EmailVerificationToken(
                    user.Id,
                    verificationToken,
                    DateTime.UtcNow.AddHours(24));

                await _emailVerificationTokenRepository.AddAsync(emailVerificationToken, cancellationToken);
                await _emailVerificationTokenRepository.SaveChangesAsync(cancellationToken);

                // Send verification email
                await _emailService.SendEmailVerificationAsync(user.Email, user.FirstName ?? "User", verificationToken);

                _logger.LogInformation("Verification email resent to: {Email}", user.Email);

                return ApiResponse.SuccessResult("If the email address exists in our system, you will receive a verification email shortly.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred during verification email resend for email: {Email}", request.Email);
                return ApiResponse.ErrorResult("An error occurred while sending the verification email. Please try again.");
            }
        }
    }
} 