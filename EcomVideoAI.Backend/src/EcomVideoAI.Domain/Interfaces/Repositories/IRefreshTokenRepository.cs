using EcomVideoAI.Domain.Entities;

namespace EcomVideoAI.Domain.Interfaces.Repositories
{
    public interface IRefreshTokenRepository : IRepository<RefreshToken>
    {
        Task<RefreshToken?> GetByTokenAsync(string token, CancellationToken cancellationToken = default);
        Task<List<RefreshToken>> GetActiveTokensForUserAsync(Guid userId, CancellationToken cancellationToken = default);
        Task RevokeAllUserTokensAsync(Guid userId, CancellationToken cancellationToken = default);
        Task RevokeTokenAsync(string token, CancellationToken cancellationToken = default);
        Task CleanupExpiredTokensAsync(CancellationToken cancellationToken = default);
    }
} 