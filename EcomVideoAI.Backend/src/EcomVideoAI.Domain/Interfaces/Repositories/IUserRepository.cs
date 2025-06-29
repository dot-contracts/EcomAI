using EcomVideoAI.Domain.Entities;

namespace EcomVideoAI.Domain.Interfaces.Repositories
{
    public interface IUserRepository : IRepository<User>
    {
        Task<User?> GetByEmailAsync(string email, CancellationToken cancellationToken = default);
        Task<bool> ExistsWithEmailAsync(string email, CancellationToken cancellationToken = default);
        Task<User?> GetWithRolesAsync(Guid userId, CancellationToken cancellationToken = default);
        Task<User?> GetWithPreferencesAsync(Guid userId, CancellationToken cancellationToken = default);
        Task<User?> GetWithSubscriptionsAsync(Guid userId, CancellationToken cancellationToken = default);
    }
} 