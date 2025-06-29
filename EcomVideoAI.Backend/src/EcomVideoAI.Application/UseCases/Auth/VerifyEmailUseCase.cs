using EcomVideoAI.Application.DTOs.Requests.Auth;
using EcomVideoAI.Application.DTOs.Responses.Common;
using EcomVideoAI.Application.Interfaces;
using EcomVideoAI.Domain.Interfaces.Repositories;
using EcomVideoAI.Domain.Interfaces.Services;
using Microsoft.Extensions.Logging;

namespace EcomVideoAI.Application.UseCases.Auth
{
    public class VerifyEmailUseCase : IAsyncUseCase<VerifyEmailRequest, ApiResponse>
    {
        private readonly IUserRepository _userRepository;
        private readonly IEmailVerificationTokenRepository _emailVerificationTokenRepository;
        private readonly IEmailService _emailService;
        private readonly ILogger<VerifyEmailUseCase> _logger;

        public VerifyEmailUseCase(
            IUserRepository userRepository,
            IEmailVerificationTokenRepository emailVerificationTokenRepository,
            IEmailService emailService,
            ILogger<VerifyEmailUseCase> logger)
        {
            _userRepository = userRepository;
            _emailVerificationTokenRepository = emailVerificationTokenRepository;
            _emailService = emailService;
            _logger = logger;
        }

        public async Task<ApiResponse> ExecuteAsync(VerifyEmailRequest request, CancellationToken cancellationToken = default)
        {
            try
            {
                // Find the verification token
                var verificationToken = await _emailVerificationTokenRepository.GetValidTokenAsync(request.Token, cancellationToken);
                
                if (verificationToken == null)
                {
                    _logger.LogWarning("Invalid or expired email verification token: {Token}", request.Token);
                    return ApiResponse.ErrorResult("Invalid or expired verification token");
                }

                // Get the user
                var user = verificationToken.User;
                
                if (user == null)
                {
                    _logger.LogWarning("User not found for verification token: {Token}", request.Token);
                    return ApiResponse.ErrorResult("Invalid verification token");
                }

                if (user.EmailVerified)
                {
                    _logger.LogInformation("Email already verified for user: {Email}", user.Email);
                    return ApiResponse.SuccessResult("Email has already been verified");
                }

                if (!user.IsActive)
                {
                    _logger.LogWarning("Email verification attempted for deactivated account: {Email}", user.Email);
                    return ApiResponse.ErrorResult("This account has been deactivated. Please contact support for assistance.");
                }

                // Mark email as verified
                user.VerifyEmail();
                
                // Mark token as used
                verificationToken.MarkAsUsed();
                
                // Save changes
                await _userRepository.SaveChangesAsync(cancellationToken);

                // Send welcome email (don't await to not block verification)
                _ = Task.Run(async () =>
                {
                    try
                    {
                        await _emailService.SendWelcomeEmailAsync(user.Email, user.FirstName ?? "User");
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning(ex, "Failed to send welcome email to {Email}", user.Email);
                    }
                }, cancellationToken);

                _logger.LogInformation("Email verified successfully for user: {Email}", user.Email);

                return ApiResponse.SuccessResult("Email verified successfully! Welcome to EcomVideo AI.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred during email verification for token: {Token}", request.Token);
                return ApiResponse.ErrorResult("An error occurred while verifying your email. Please try again.");
            }
        }
    }
} 