using EcomVideoAI.Domain.Entities;
using EcomVideoAI.Domain.Interfaces.Repositories;
using EcomVideoAI.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace EcomVideoAI.Infrastructure.Repositories
{
    public class RefreshTokenRepository : GenericRepository<RefreshToken>, IRefreshTokenRepository
    {
        public RefreshTokenRepository(ApplicationDbContext context) : base(context) { }

        public async Task<RefreshToken?> GetByTokenAsync(string token, CancellationToken cancellationToken = default)
        {
            return await _dbSet
                .Include(rt => rt.User)
                .FirstOrDefaultAsync(rt => rt.Token == token, cancellationToken);
        }

        public async Task<List<RefreshToken>> GetActiveTokensForUserAsync(Guid userId, CancellationToken cancellationToken = default)
        {
            return await _dbSet
                .Where(rt => rt.UserId == userId && rt.IsActive)
                .ToListAsync(cancellationToken);
        }

        public async Task RevokeAllUserTokensAsync(Guid userId, CancellationToken cancellationToken = default)
        {
            var tokens = await _dbSet
                .Where(rt => rt.UserId == userId && !rt.IsRevoked)
                .ToListAsync(cancellationToken);

            foreach (var token in tokens)
            {
                token.Revoke();
            }

            await _context.SaveChangesAsync(cancellationToken);
        }

        public async Task RevokeTokenAsync(string token, CancellationToken cancellationToken = default)
        {
            var refreshToken = await _dbSet
                .FirstOrDefaultAsync(rt => rt.Token == token, cancellationToken);

            if (refreshToken != null && !refreshToken.IsRevoked)
            {
                refreshToken.Revoke();
                await _context.SaveChangesAsync(cancellationToken);
            }
        }

        public async Task CleanupExpiredTokensAsync(CancellationToken cancellationToken = default)
        {
            var expiredTokens = await _dbSet
                .Where(rt => rt.ExpiresAt < DateTime.UtcNow)
                .ToListAsync(cancellationToken);

            _dbSet.RemoveRange(expiredTokens);
            await _context.SaveChangesAsync(cancellationToken);
        }
    }
} 