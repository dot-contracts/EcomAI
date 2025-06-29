using EcomVideoAI.Domain.Entities;
using EcomVideoAI.Domain.Interfaces.Repositories;
using EcomVideoAI.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace EcomVideoAI.Infrastructure.Repositories
{
    public class EmailVerificationTokenRepository : GenericRepository<EmailVerificationToken>, IEmailVerificationTokenRepository
    {
        public EmailVerificationTokenRepository(ApplicationDbContext context) : base(context) { }

        public async Task<EmailVerificationToken?> GetByTokenAsync(string token, CancellationToken cancellationToken = default)
        {
            return await _dbSet
                .Include(evt => evt.User)
                .FirstOrDefaultAsync(evt => evt.Token == token, cancellationToken);
        }

        public async Task<EmailVerificationToken?> GetValidTokenAsync(string token, CancellationToken cancellationToken = default)
        {
            return await _dbSet
                .Include(evt => evt.User)
                .FirstOrDefaultAsync(evt => evt.Token == token && evt.IsValid, cancellationToken);
        }

        public async Task<List<EmailVerificationToken>> GetActiveTokensForUserAsync(Guid userId, CancellationToken cancellationToken = default)
        {
            return await _dbSet
                .Where(evt => evt.UserId == userId && evt.IsValid)
                .ToListAsync(cancellationToken);
        }

        public async Task RevokeAllUserTokensAsync(Guid userId, CancellationToken cancellationToken = default)
        {
            var tokens = await _dbSet
                .Where(evt => evt.UserId == userId && !evt.IsUsed)
                .ToListAsync(cancellationToken);

            foreach (var token in tokens)
            {
                token.MarkAsUsed();
            }

            await _context.SaveChangesAsync(cancellationToken);
        }

        public async Task CleanupExpiredTokensAsync(CancellationToken cancellationToken = default)
        {
            var expiredTokens = await _dbSet
                .Where(evt => evt.IsExpired)
                .ToListAsync(cancellationToken);

            _dbSet.RemoveRange(expiredTokens);
            await _context.SaveChangesAsync(cancellationToken);
        }
    }
} 