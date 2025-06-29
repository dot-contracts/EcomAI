using EcomVideoAI.Application.DTOs.Requests.Video;
using EcomVideoAI.Application.DTOs.Responses.Video;
using EcomVideoAI.Application.Interfaces;
using EcomVideoAI.Domain.Entities;
using EcomVideoAI.Domain.Enums;
using EcomVideoAI.Domain.Interfaces.Repositories;
using EcomVideoAI.Domain.Interfaces.Services;
using Microsoft.Extensions.Logging;

namespace EcomVideoAI.Application.UseCases.Video
{
    public class CreateVideoFromTextUseCase : IAsyncUseCase<CreateVideoFromTextRequest, VideoResponse>
    {
        private readonly IVideoRepository _videoRepository;
        private readonly IFreepikService _freepikService;
        private readonly ILogger<CreateVideoFromTextUseCase> _logger;

        public CreateVideoFromTextUseCase(
            IVideoRepository videoRepository,
            IFreepikService freepikService,
            ILogger<CreateVideoFromTextUseCase> logger)
        {
            _videoRepository = videoRepository;
            _freepikService = freepikService;
            _logger = logger;
        }

        public async Task<VideoResponse> ExecuteAsync(CreateVideoFromTextRequest request, CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("Starting video creation from text for user {UserId} with prompt: {Prompt}, AspectRatio: {AspectRatio}, Style: {Style}", 
                request.UserId, request.TextPrompt, request.AspectRatio, request.Style);

            try
            {
                // Convert aspect ratio string to enum
                var aspectRatio = AspectRatioExtensions.FromString(request.AspectRatio);

                // Create video entity
                var video = new Domain.Entities.Video(
                    userId: request.UserId,
                    title: request.Title,
                    description: request.Description,
                    textPrompt: request.TextPrompt,
                    inputType: VideoInputType.Text,
                    resolution: request.Resolution,
                    aspectRatio: aspectRatio,
                    style: request.Style,
                    durationSeconds: request.Duration)
                {
                    Title = request.Title,
                    Description = request.Description,
                    TextPrompt = request.TextPrompt
                };

                // Save to database with Pending status
                await _videoRepository.AddAsync(video, cancellationToken);
                await _videoRepository.SaveChangesAsync(cancellationToken);

                _logger.LogInformation("Video entity created with ID {VideoId} and status {Status}. VideoProcessingService will handle direct text-to-video generation.", 
                    video.Id, video.Status);

                return MapToResponse(video);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating video from text for user {UserId}", request.UserId);
                throw;
            }
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