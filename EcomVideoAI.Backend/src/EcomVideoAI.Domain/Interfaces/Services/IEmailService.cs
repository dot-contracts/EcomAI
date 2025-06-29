namespace EcomVideoAI.Domain.Interfaces.Services
{
    public interface IEmailService
    {
        Task SendWelcomeEmailAsync(string email, string firstName);
        Task SendEmailVerificationAsync(string email, string firstName, string verificationToken);
        Task SendPasswordResetEmailAsync(string email, string firstName, string resetToken);
        Task SendPasswordChangedNotificationAsync(string email, string firstName);
        Task<bool> SendEmailAsync(string to, string subject, string body, bool isHtml = true);
    }
} 