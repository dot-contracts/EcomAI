using EcomVideoAI.Domain.Interfaces.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace EcomVideoAI.Infrastructure.Services
{
    public class EmailService : IEmailService
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<EmailService> _logger;

        public EmailService(IConfiguration configuration, ILogger<EmailService> logger)
        {
            _configuration = configuration;
            _logger = logger;
        }

        public async Task SendWelcomeEmailAsync(string email, string firstName)
        {
            var subject = "Welcome to EcomVideo AI!";
            var body = $@"
                <html>
                <body>
                    <h2>Welcome to EcomVideo AI, {firstName}!</h2>
                    <p>Thank you for joining EcomVideo AI. We're excited to help you create amazing promotional videos for your products.</p>
                    <p>Get started by:</p>
                    <ul>
                        <li>Uploading your first product image or URL</li>
                        <li>Exploring our video templates</li>
                        <li>Creating your first AI-powered promotional video</li>
                    </ul>
                    <p>If you have any questions, feel free to contact our support team.</p>
                    <p>Best regards,<br>The EcomVideo AI Team</p>
                </body>
                </html>";

            await SendEmailAsync(email, subject, body, true);
        }

        public async Task SendEmailVerificationAsync(string email, string firstName, string verificationToken)
        {
            var verificationUrl = $"{_configuration["Frontend:Url"]}/verify-email?token={verificationToken}";
            var subject = "Verify Your Email Address";
            var body = $@"
                <html>
                <body>
                    <h2>Hi {firstName},</h2>
                    <p>Please verify your email address by clicking the link below:</p>
                    <p><a href='{verificationUrl}' style='background-color: #4CAF50; color: white; padding: 12px 24px; text-decoration: none; border-radius: 4px; display: inline-block;'>Verify Email</a></p>
                    <p>If the button doesn't work, copy and paste this link into your browser:</p>
                    <p>{verificationUrl}</p>
                    <p>This link will expire in 24 hours.</p>
                    <p>Best regards,<br>The EcomVideo AI Team</p>
                </body>
                </html>";

            await SendEmailAsync(email, subject, body, true);
        }

        public async Task SendPasswordResetEmailAsync(string email, string firstName, string resetToken)
        {
            var resetUrl = $"{_configuration["Frontend:Url"]}/reset-password?token={resetToken}";
            var subject = "Reset Your Password";
            var body = $@"
                <html>
                <body>
                    <h2>Hi {firstName},</h2>
                    <p>You requested to reset your password. Click the link below to set a new password:</p>
                    <p><a href='{resetUrl}' style='background-color: #FF6B35; color: white; padding: 12px 24px; text-decoration: none; border-radius: 4px; display: inline-block;'>Reset Password</a></p>
                    <p>If the button doesn't work, copy and paste this link into your browser:</p>
                    <p>{resetUrl}</p>
                    <p>This link will expire in 1 hour. If you didn't request this reset, please ignore this email.</p>
                    <p>Best regards,<br>The EcomVideo AI Team</p>
                </body>
                </html>";

            await SendEmailAsync(email, subject, body, true);
        }

        public async Task SendPasswordChangedNotificationAsync(string email, string firstName)
        {
            var subject = "Password Changed Successfully";
            var body = $@"
                <html>
                <body>
                    <h2>Hi {firstName},</h2>
                    <p>Your password has been successfully changed.</p>
                    <p>If you didn't make this change, please contact our support team immediately.</p>
                    <p>Best regards,<br>The EcomVideo AI Team</p>
                </body>
                </html>";

            await SendEmailAsync(email, subject, body, true);
        }

        public async Task<bool> SendEmailAsync(string to, string subject, string body, bool isHtml = true)
        {
            try
            {
                // TODO: Implement actual email sending using SendGrid, AWS SES, or SMTP
                // For now, just log the email content
                _logger.LogInformation("Email would be sent to {Email} with subject: {Subject}", to, subject);
                _logger.LogDebug("Email body: {Body}", body);

                // Simulate async email sending
                await Task.Delay(100);

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send email to {Email}", to);
                return false;
            }
        }
    }
} 