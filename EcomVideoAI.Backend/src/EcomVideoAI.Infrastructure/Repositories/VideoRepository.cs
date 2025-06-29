using EcomVideoAI.Domain.Entities;
using EcomVideoAI.Domain.Enums;
using EcomVideoAI.Domain.Interfaces.Repositories;
using EcomVideoAI.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace EcomVideoAI.Infrastructure.Repositories
{
    public class VideoRepository : GenericRepository<Video>, IVideoRepository
    {
        public VideoRepository(ApplicationDbContext context) : base(context)
        {
        }

        public async Task<Video?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        {
            return await _context.Videos
                .FirstOrDefaultAsync(v => v.Id == id, cancellationToken);
        }

        public async Task<IEnumerable<Video>> GetByUserIdAsync(Guid userId, int page, int pageSize, CancellationToken cancellationToken = default)
        {
            return await _context.Videos
                .Where(v => v.UserId == userId)
                .OrderByDescending(v => v.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync(cancellationToken);
        }

        public async Task<IEnumerable<Video>> GetByStatusAsync(VideoStatus status, CancellationToken cancellationToken = default)
        {
            return await _context.Videos
                .Where(v => v.Status == status)
                .OrderBy(v => v.CreatedAt)
                .ToListAsync(cancellationToken);
        }

        public async Task<IEnumerable<Video>> GetByStatus(IEnumerable<VideoStatus> statuses, CancellationToken cancellationToken = default)
        {
            return await _context.Videos
                .Where(v => statuses.Contains(v.Status))
                .OrderBy(v => v.CreatedAt)
                .ToListAsync(cancellationToken);
        }

        public async Task<Video?> GetByFreepikTaskIdAsync(string taskId, CancellationToken cancellationToken = default)
        {
            return await _context.Videos
                .FirstOrDefaultAsync(v => v.FreepikTaskId == taskId, cancellationToken);
        }

        public async Task<Video?> GetByFreepikImageTaskIdAsync(string taskId, CancellationToken cancellationToken = default)
        {
            return await _context.Videos
                .FirstOrDefaultAsync(v => v.FreepikImageTaskId == taskId, cancellationToken);
        }

        public async Task<IEnumerable<Video>> GetPendingVideosAsync(CancellationToken cancellationToken = default)
        {
            return await _context.Videos
                .Where(v => v.Status == VideoStatus.Pending || 
                           v.Status == VideoStatus.Processing ||
                           v.Status == VideoStatus.GeneratingImage ||
                           v.Status == VideoStatus.GeneratingVideo)
                .OrderBy(v => v.CreatedAt)
                .ToListAsync(cancellationToken);
        }

        public async Task<IEnumerable<Video>> GetProcessingVideosAsync(CancellationToken cancellationToken = default)
        {
            return await _context.Videos
                .Where(v => v.Status == VideoStatus.Processing ||
                           v.Status == VideoStatus.GeneratingImage ||
                           v.Status == VideoStatus.GeneratingVideo)
                .OrderBy(v => v.CreatedAt)
                .ToListAsync(cancellationToken);
        }

        public async Task<int> GetUserVideoCountAsync(Guid userId, CancellationToken cancellationToken = default)
        {
            return await _context.Videos
                .CountAsync(v => v.UserId == userId, cancellationToken);
        }

        public async Task<IEnumerable<Video>> SearchVideosAsync(string searchTerm, Guid? userId = null, CancellationToken cancellationToken = default)
        {
            var query = _context.Videos.AsQueryable();

            if (userId.HasValue)
            {
                query = query.Where(v => v.UserId == userId.Value);
            }

            if (!string.IsNullOrEmpty(searchTerm))
            {
                query = query.Where(v => 
                    v.Title.Contains(searchTerm) || 
                    v.Description.Contains(searchTerm) || 
                    v.TextPrompt.Contains(searchTerm));
            }

            return await query
                .OrderByDescending(v => v.CreatedAt)
                .ToListAsync(cancellationToken);
        }
    }
} 