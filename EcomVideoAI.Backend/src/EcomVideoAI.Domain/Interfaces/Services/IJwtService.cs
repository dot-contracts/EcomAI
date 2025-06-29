using EcomVideoAI.Domain.Entities;
using System.Security.Claims;

namespace EcomVideoAI.Domain.Interfaces.Services
{
    public interface IJwtService
    {
        string GenerateAccessToken(User user, List<string> roles);
        string GenerateRefreshToken();
        ClaimsPrincipal? ValidateToken(string token, bool validateLifetime = true);
        Guid? GetUserIdFromToken(string token);
        DateTime GetTokenExpiration(string token);
        bool IsTokenExpired(string token);
    }
} 