using EcomVideoAI.Domain.Entities;
using EcomVideoAI.Domain.Enums;
using EcomVideoAI.Domain.Interfaces.Repositories;
using EcomVideoAI.Domain.Interfaces.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;

namespace EcomVideoAI.Infrastructure.BackgroundServices
{
    public class VideoProcessingService : BackgroundService
    {
        private readonly ILogger<VideoProcessingService> _logger;
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly IConfiguration _configuration;
        private readonly TimeSpan _processingInterval;

        public VideoProcessingService(
            ILogger<VideoProcessingService> logger, 
            IServiceScopeFactory scopeFactory,
            IConfiguration configuration)
        {
            _logger = logger;
            _scopeFactory = scopeFactory;
            _configuration = configuration;
            
            // Get processing interval from configuration, default to 30 seconds
            var intervalString = _configuration["BackgroundServices:VideoProcessingInterval"] ?? "00:00:30";
            _processingInterval = TimeSpan.Parse(intervalString);
            
            _logger.LogInformation("VideoProcessingService initialized with interval: {Interval}", _processingInterval);
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Video Processing Service is starting.");

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    await ProcessPendingVideos(stoppingToken);
                }
                catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
                {
                    _logger.LogInformation("Video Processing Service is being cancelled.");
                    break;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "An error occurred while processing videos. Service will continue.");
                }

                try
                {
                    await Task.Delay(_processingInterval, stoppingToken);
                }
                catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
                {
                    _logger.LogInformation("Video Processing Service delay cancelled.");
                    break;
                }
            }

            _logger.LogInformation("Video Processing Service is stopping.");
        }

        private async Task ProcessPendingVideos(CancellationToken stoppingToken)
        {
            try
            {
                using var scope = _scopeFactory.CreateScope();
                var videoRepository = scope.ServiceProvider.GetRequiredService<IVideoRepository>();
                var freepikService = scope.ServiceProvider.GetRequiredService<IFreepikService>();

                var videosToProcess = await videoRepository.GetByStatus(new[] { VideoStatus.Pending, VideoStatus.GeneratingImage, VideoStatus.GeneratingVideo });

                if (!videosToProcess.Any())
                {
                    _logger.LogDebug("No videos to process.");
                    return;
                }
                
                _logger.LogInformation("Found {Count} videos to process.", videosToProcess.Count());

                foreach (var video in videosToProcess)
                {
                    if (stoppingToken.IsCancellationRequested)
                    {
                        _logger.LogInformation("Processing cancelled for video {VideoId}.", video.Id);
                        break;
                    }

                    try
                    {
                        await ProcessSingleVideo(video, videoRepository, freepikService, stoppingToken);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Failed to process video {VideoId}. Moving to next video.", video.Id);
                        try
                        {
                            video.SetError($"Processing failed: {ex.Message}");
                            await videoRepository.UpdateAsync(video, stoppingToken);
                        }
                        catch (Exception updateEx)
                        {
                            _logger.LogError(updateEx, "Failed to update video {VideoId} with error status.", video.Id);
                        }
                    }
                }

                try
                {
                    await videoRepository.SaveChangesAsync(stoppingToken);
                    _logger.LogDebug("Saved changes to video repository.");
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to save changes to video repository.");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in ProcessPendingVideos method.");
                throw;
            }
        }

        private async Task ProcessSingleVideo(
            Video video, 
            IVideoRepository videoRepository, 
            IFreepikService freepikService, 
            CancellationToken stoppingToken)
        {
            _logger.LogInformation("Processing video {VideoId} with status {Status}", video.Id, video.Status);

            if (video.Status == VideoStatus.Pending)
            {
                _logger.LogInformation("Starting image generation from text for video {VideoId}", video.Id);
                
                // Step 1: Generate image from text prompt
                var imageTask = await freepikService.GenerateImageFromTextAsync(
                    video.TextPrompt,
                    null, // negative prompt
                    null, // style
                    video.Resolution,
                    stoppingToken);

                if (imageTask != null && !string.IsNullOrEmpty(imageTask.TaskId))
                {
                    video.SetFreepikImageTaskId(imageTask.TaskId);
                    video.UpdateStatus(VideoStatus.GeneratingImage);
                    await videoRepository.UpdateAsync(video, stoppingToken);
                    _logger.LogInformation("Image generation started for video {VideoId} with task ID {TaskId}", 
                        video.Id, imageTask.TaskId);
                }
                else
                {
                    var errorMessage = imageTask?.ErrorMessage ?? "Failed to start image generation - no task ID received";
                    video.SetError(errorMessage);
                    await videoRepository.UpdateAsync(video, stoppingToken);
                    _logger.LogError("Failed to start image generation for video {VideoId}: {Error}", video.Id, errorMessage);
                }
            }
            else if (video.Status == VideoStatus.GeneratingImage && !string.IsNullOrEmpty(video.FreepikImageTaskId))
            {
                _logger.LogInformation("Checking image generation status for video {VideoId} with task ID {TaskId}", 
                    video.Id, video.FreepikImageTaskId);
                
                var imageStatus = await freepikService.GetImageTaskStatusAsync(video.FreepikImageTaskId, stoppingToken);

                switch (imageStatus.Status)
                {
                    case FreepikTaskStatusType.Completed:
                        _logger.LogInformation("Image generation completed for video {VideoId}", video.Id);
                        var imageResult = await freepikService.GetImageResultAsync(video.FreepikImageTaskId, stoppingToken);
                        if (imageResult != null && !string.IsNullOrEmpty(imageResult.ImageUrl))
                        {
                            video.SetImageUrl(imageResult.ImageUrl);
                            _logger.LogInformation("Image URL set for video {VideoId}: {ImageUrl}", video.Id, imageResult.ImageUrl);
                            
                            // Step 2: Generate video from the generated image
                            _logger.LogInformation("Starting video generation from image for video {VideoId}", video.Id);
                            var videoTask = await freepikService.GenerateVideoFromImageAsync(
                                imageResult.ImageUrl,
                                video.TextPrompt, // Use original prompt as additional context
                                video.Resolution,
                                video.DurationSeconds,
                                stoppingToken);

                            if (videoTask != null && !string.IsNullOrEmpty(videoTask.TaskId))
                            {
                                video.SetFreepikVideoTaskId(videoTask.TaskId);
                                video.UpdateStatus(VideoStatus.GeneratingVideo);
                                await videoRepository.UpdateAsync(video, stoppingToken);
                                _logger.LogInformation("Video generation started for video {VideoId} with task ID {TaskId}", 
                                    video.Id, videoTask.TaskId);
                            }
                            else
                            {
                                var errorMessage = videoTask?.ErrorMessage ?? "Failed to start video generation - no task ID received";
                                video.SetError(errorMessage);
                                await videoRepository.UpdateAsync(video, stoppingToken);
                                _logger.LogError("Failed to start video generation for video {VideoId}: {Error}", video.Id, errorMessage);
                            }
                        }
                        else
                        {
                            video.SetError("Failed to retrieve image result after completion.");
                            await videoRepository.UpdateAsync(video, stoppingToken);
                            _logger.LogError("Failed to retrieve image result for video {VideoId}", video.Id);
                        }
                        break;
                        
                    case FreepikTaskStatusType.Failed:
                        _logger.LogError("Image generation failed for video {VideoId}: {Error}", 
                            video.Id, imageStatus.ErrorMessage);
                        video.SetError(imageStatus.ErrorMessage ?? "Image generation failed");
                        await videoRepository.UpdateAsync(video, stoppingToken);
                        break;
                        
                    case FreepikTaskStatusType.InProgress:
                        _logger.LogDebug("Image generation still in progress for video {VideoId}", video.Id);
                        break;
                        
                    default:
                        _logger.LogDebug("Image generation status {Status} for video {VideoId}", 
                            imageStatus.Status, video.Id);
                        break;
                }
            }
            else if (video.Status == VideoStatus.GeneratingImage && string.IsNullOrEmpty(video.FreepikImageTaskId))
            {
                // Handle videos stuck in GeneratingImage status without task ID - reset to Pending
                _logger.LogWarning("Video {VideoId} is in GeneratingImage status but has no image task ID. Resetting to Pending.", video.Id);
                video.UpdateStatus(VideoStatus.Pending);
                await videoRepository.UpdateAsync(video, stoppingToken);
            }
            else if (video.Status == VideoStatus.GeneratingVideo && !string.IsNullOrEmpty(video.FreepikTaskId))
            {
                _logger.LogInformation("Checking video generation status for video {VideoId} with task ID {TaskId}", 
                    video.Id, video.FreepikTaskId);
                
                var videoStatus = await freepikService.GetVideoTaskStatusAsync(video.FreepikTaskId, stoppingToken);

                switch (videoStatus.Status)
                {
                    case FreepikTaskStatusType.Completed:
                        _logger.LogInformation("Video generation completed for video {VideoId}", video.Id);
                        var videoResult = await freepikService.GetVideoResultAsync(video.FreepikTaskId, stoppingToken);
                        if (videoResult != null && !string.IsNullOrEmpty(videoResult.VideoUrl))
                        {
                            video.SetVideoUrl(videoResult.VideoUrl, videoResult.FileSizeBytes);
                            video.UpdateStatus(VideoStatus.Completed);
                            _logger.LogInformation("Video {VideoId} completed successfully with URL {VideoUrl}", 
                                video.Id, videoResult.VideoUrl);
                        }
                        else
                        {
                            video.SetError("Failed to retrieve video result after completion.");
                        }
                        await videoRepository.UpdateAsync(video, stoppingToken);
                        break;
                        
                    case FreepikTaskStatusType.Failed:
                        _logger.LogError("Video generation failed for video {VideoId}: {Error}", 
                            video.Id, videoStatus.ErrorMessage);
                        video.SetError(videoStatus.ErrorMessage ?? "Video generation failed");
                        await videoRepository.UpdateAsync(video, stoppingToken);
                        break;
                        
                    case FreepikTaskStatusType.InProgress:
                        _logger.LogDebug("Video generation still in progress for video {VideoId}", video.Id);
                        break;
                        
                    default:
                        _logger.LogDebug("Video generation status {Status} for video {VideoId}", 
                            videoStatus.Status, video.Id);
                        break;
                }
            }
            else
            {
                _logger.LogWarning("Video {VideoId} in unexpected state: Status={Status}, ImageTaskId={ImageTaskId}, VideoTaskId={VideoTaskId}", 
                    video.Id, video.Status, video.FreepikImageTaskId, video.FreepikTaskId);
            }
        }
    }
} 