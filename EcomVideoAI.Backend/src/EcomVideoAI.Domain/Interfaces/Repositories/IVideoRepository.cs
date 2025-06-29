using EcomVideoAI.Domain.Entities;
using EcomVideoAI.Domain.Enums;

namespace EcomVideoAI.Domain.Interfaces.Repositories
{
    public interface IVideoRepository : IRepository<Video>
    {
        Task<Video?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
        Task<IEnumerable<Video>> GetByUserIdAsync(Guid userId, int page, int pageSize, CancellationToken cancellationToken = default);
        Task<IEnumerable<Video>> GetByStatusAsync(VideoStatus status, CancellationToken cancellationToken = default);
        Task<IEnumerable<Video>> GetByStatus(IEnumerable<VideoStatus> statuses, CancellationToken cancellationToken = default);
        Task<Video?> GetByFreepikTaskIdAsync(string taskId, CancellationToken cancellationToken = default);
        Task<Video?> GetByFreepikImageTaskIdAsync(string taskId, CancellationToken cancellationToken = default);
        Task<IEnumerable<Video>> GetPendingVideosAsync(CancellationToken cancellationToken = default);
        Task<IEnumerable<Video>> GetProcessingVideosAsync(CancellationToken cancellationToken = default);
        Task<int> GetUserVideoCountAsync(Guid userId, CancellationToken cancellationToken = default);
        Task<IEnumerable<Video>> SearchVideosAsync(string searchTerm, Guid? userId = null, CancellationToken cancellationToken = default);
    }
} 