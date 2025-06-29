namespace EcomVideoAI.Domain.Interfaces.Services
{
    public interface IPasswordService
    {
        string HashPassword(string password);
        bool VerifyPassword(string password, string hashedPassword);
        string GenerateSecureToken(int length = 32);
    }
} 