using EcomVideoAI.Domain.Interfaces.Repositories;
using Microsoft.Extensions.Logging;

namespace EcomVideoAI.Application.UseCases.Video
{
    public class GetVideoStatusUseCase
    {
        private readonly IVideoRepository _videoRepository;
        private readonly ILogger<GetVideoStatusUseCase> _logger;

        public GetVideoStatusUseCase(
            IVideoRepository videoRepository,
            ILogger<GetVideoStatusUseCase> logger)
        {
            _videoRepository = videoRepository;
            _logger = logger;
        }

        public async Task<VideoStatusResponse?> ExecuteAsync(Guid videoId, CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("Getting status for video with ID {VideoId}", videoId);

            var video = await _videoRepository.GetByIdAsync(videoId, cancellationToken);
            
            if (video == null)
            {
                _logger.LogWarning("Video with ID {VideoId} not found", videoId);
                return null;
            }

            // Calculate progress based on status
            var progress = CalculateProgress(video.Status);

            return new VideoStatusResponse
            {
                Id = video.Id,
                Status = video.Status.ToString(),
                Progress = progress,
                Message = GetStatusMessage(video.Status)
            };
        }

        private static int CalculateProgress(Domain.Enums.VideoStatus status)
        {
            return status switch
            {
                Domain.Enums.VideoStatus.Pending => 10,
                Domain.Enums.VideoStatus.GeneratingImage => 30,
                Domain.Enums.VideoStatus.GeneratingVideo => 60,
                Domain.Enums.VideoStatus.Processing => 80,
                Domain.Enums.VideoStatus.Completed => 100,
                Domain.Enums.VideoStatus.Failed => 0,
                _ => 0
            };
        }

        private static string GetStatusMessage(Domain.Enums.VideoStatus status)
        {
            return status switch
            {
                Domain.Enums.VideoStatus.Pending => "Preparing your video...",
                Domain.Enums.VideoStatus.GeneratingImage => "Creating image from your prompt...",
                Domain.Enums.VideoStatus.GeneratingVideo => "Generating video from image...",
                Domain.Enums.VideoStatus.Processing => "Finalizing your video...",
                Domain.Enums.VideoStatus.Completed => "Video generation completed!",
                Domain.Enums.VideoStatus.Failed => "Video generation failed",
                _ => "Processing your request..."
            };
        }
    }

    public class VideoStatusResponse
    {
        public Guid Id { get; set; }
        public string Status { get; set; } = string.Empty;
        public int Progress { get; set; }
        public string Message { get; set; } = string.Empty;
    }
} 