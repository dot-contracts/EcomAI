using EcomVideoAI.Application.DTOs.Responses.Video;
using EcomVideoAI.Domain.Interfaces.Repositories;
using Microsoft.Extensions.Logging;

namespace EcomVideoAI.Application.UseCases.Video
{
    public class GetVideoByIdUseCase
    {
        private readonly IVideoRepository _videoRepository;
        private readonly ILogger<GetVideoByIdUseCase> _logger;

        public GetVideoByIdUseCase(
            IVideoRepository videoRepository,
            ILogger<GetVideoByIdUseCase> logger)
        {
            _videoRepository = videoRepository;
            _logger = logger;
        }

        public async Task<VideoResponse?> ExecuteAsync(Guid videoId, CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("Getting video with ID {VideoId}", videoId);

            var video = await _videoRepository.GetByIdAsync(videoId, cancellationToken);
            
            if (video == null)
            {
                _logger.LogWarning("Video with ID {VideoId} not found", videoId);
                return null;
            }

            return MapToResponse(video);
        }

        private static VideoResponse MapToResponse(Domain.Entities.Video video)
        {
            return new VideoResponse
            {
                Id = video.Id,
                UserId = video.UserId,
                Title = video.Title,
                Description = video.Description,
                TextPrompt = video.TextPrompt,
                InputType = video.InputType,
                Status = video.Status,
                Resolution = video.Resolution,
                DurationSeconds = video.DurationSeconds,
                ImageUrl = video.ImageUrl,
                VideoUrl = video.VideoUrl,
                ThumbnailUrl = video.ThumbnailUrl,
                CompletedAt = video.CompletedAt,
                ErrorMessage = video.ErrorMessage,
                FileSizeBytes = video.FileSizeBytes,
                Metadata = video.Metadata != null ? new VideoMetadataResponse
                {
                    Width = video.Metadata.Width,
                    Height = video.Metadata.Height,
                    FrameRate = video.Metadata.FrameRate,
                    Format = video.Metadata.Format,
                    Codec = video.Metadata.Codec,
                    Bitrate = video.Metadata.Bitrate,
                    AspectRatio = video.Metadata.GetAspectRatio()
                } : null,
                CreatedAt = video.CreatedAt,
                UpdatedAt = video.UpdatedAt
            };
        }
    }
} 