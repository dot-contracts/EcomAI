using EcomVideoAI.Domain.Entities;

namespace EcomVideoAI.Domain.Interfaces.Repositories
{
    public interface IEmailVerificationTokenRepository : IRepository<EmailVerificationToken>
    {
        Task<EmailVerificationToken?> GetByTokenAsync(string token, CancellationToken cancellationToken = default);
        Task<EmailVerificationToken?> GetValidTokenAsync(string token, CancellationToken cancellationToken = default);
        Task<List<EmailVerificationToken>> GetActiveTokensForUserAsync(Guid userId, CancellationToken cancellationToken = default);
        Task RevokeAllUserTokensAsync(Guid userId, CancellationToken cancellationToken = default);
        Task CleanupExpiredTokensAsync(CancellationToken cancellationToken = default);
    }
} 