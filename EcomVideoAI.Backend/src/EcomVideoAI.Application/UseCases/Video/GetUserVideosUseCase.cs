using EcomVideoAI.Application.DTOs.Responses.Video;
using EcomVideoAI.Domain.Entities;
using EcomVideoAI.Domain.Interfaces.Repositories;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Threading;
using System;

namespace EcomVideoAI.Application.UseCases.Video
{
    public class GetUserVideosUseCase
    {
        private readonly IVideoRepository _videoRepository;

        public GetUserVideosUseCase(IVideoRepository videoRepository)
        {
            _videoRepository = videoRepository;
        }

        public async Task<IEnumerable<VideoResponse>> ExecuteAsync(Guid userId, int page, int pageSize, CancellationToken cancellationToken)
        {
            var videos = await _videoRepository.GetByUserIdAsync(userId, page, pageSize, cancellationToken);
            
            // Manual mapping from Video entity to VideoResponse DTO
            return videos.Select(video => new VideoResponse
            {
                Id = video.Id,
                UserId = video.UserId,
                Title = video.Title,
                Status = video.Status,
                VideoUrl = video.VideoUrl,
                CreatedAt = video.CreatedAt
            });
        }
    }
} 