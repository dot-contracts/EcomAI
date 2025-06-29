using EcomVideoAI.Domain.Entities;
using EcomVideoAI.Domain.Interfaces.Repositories;
using EcomVideoAI.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace EcomVideoAI.Infrastructure.Repositories
{
    public class UserRepository : GenericRepository<User>, IUserRepository
    {
        public UserRepository(ApplicationDbContext context) : base(context) { }

        public async Task<User?> GetByEmailAsync(string email, CancellationToken cancellationToken = default)
        {
            return await _dbSet
                .FirstOrDefaultAsync(u => u.Email == email.ToLowerInvariant(), cancellationToken);
        }

        public async Task<bool> ExistsWithEmailAsync(string email, CancellationToken cancellationToken = default)
        {
            return await _dbSet
                .AnyAsync(u => u.Email == email.ToLowerInvariant(), cancellationToken);
        }

        public async Task<User?> GetWithRolesAsync(Guid userId, CancellationToken cancellationToken = default)
        {
            return await _dbSet
                .Include(u => u.Roles)
                .FirstOrDefaultAsync(u => u.Id == userId, cancellationToken);
        }

        public async Task<User?> GetWithPreferencesAsync(Guid userId, CancellationToken cancellationToken = default)
        {
            return await _dbSet
                .Include(u => u.Preferences)
                .Include(u => u.NotificationSettings)
                .FirstOrDefaultAsync(u => u.Id == userId, cancellationToken);
        }

        public async Task<User?> GetWithSubscriptionsAsync(Guid userId, CancellationToken cancellationToken = default)
        {
            return await _dbSet
                .Include(u => u.Subscriptions)
                    .ThenInclude(s => s.SubscriptionPlan)
                .Include(u => u.PaymentMethods)
                .FirstOrDefaultAsync(u => u.Id == userId, cancellationToken);
        }
    }
} 