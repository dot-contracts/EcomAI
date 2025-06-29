using EcomVideoAI.Application.DTOs.Requests.Video;
using EcomVideoAI.Application.DTOs.Responses.Video;
using EcomVideoAI.Application.UseCases.Video;
using EcomVideoAI.Domain.Enums;
using Microsoft.AspNetCore.Mvc;
using static EcomVideoAI.Application.UseCases.Video.GetVideoStatusUseCase;

namespace EcomVideoAI.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class VideoController : ControllerBase
    {
        private readonly CreateVideoFromTextUseCase _createVideoFromTextUseCase;
        private readonly GetUserVideosUseCase _getUserVideosUseCase;
        private readonly GetVideoByIdUseCase _getVideoByIdUseCase;
        private readonly GetVideoStatusUseCase _getVideoStatusUseCase;
        private readonly ILogger<VideoController> _logger;

        public VideoController(
            CreateVideoFromTextUseCase createVideoFromTextUseCase,
            GetUserVideosUseCase getUserVideosUseCase,
            GetVideoByIdUseCase getVideoByIdUseCase,
            GetVideoStatusUseCase getVideoStatusUseCase,
            ILogger<VideoController> logger)
        {
            _createVideoFromTextUseCase = createVideoFromTextUseCase;
            _getUserVideosUseCase = getUserVideosUseCase;
            _getVideoByIdUseCase = getVideoByIdUseCase;
            _getVideoStatusUseCase = getVideoStatusUseCase;
            _logger = logger;
        }

        /// <summary>
        /// Creates a video from text prompt
        /// </summary>
        [HttpPost("create-from-text")]
        public async Task<ActionResult<VideoResponse>> CreateVideoFromText(
            [FromBody] CreateVideoFromTextRequest request,
            CancellationToken cancellationToken = default)
        {
            try
            {
                _logger.LogInformation("Received video creation request for user {UserId}", request.UserId);
                
                var result = await _createVideoFromTextUseCase.ExecuteAsync(request, cancellationToken);
                
                return Ok(result);
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning("Invalid request parameters: {Error}", ex.Message);
                return BadRequest(new { error = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating video from text");
                return StatusCode(500, new { error = "An error occurred while processing your request" });
            }
        }

        /// <summary>
        /// Gets video by ID
        /// </summary>
        [HttpGet("{id:guid}")]
        public async Task<ActionResult<VideoResponse>> GetVideo(
            Guid id, 
            CancellationToken cancellationToken = default)
        {
            try
            {
                _logger.LogInformation("Getting video with ID {VideoId}", id);
                
                var result = await _getVideoByIdUseCase.ExecuteAsync(id, cancellationToken);
                
                if (result == null)
                {
                    return NotFound(new { error = "Video not found" });
                }
                
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting video {VideoId}", id);
                return StatusCode(500, new { error = "An error occurred while retrieving the video" });
            }
        }

        /// <summary>
        /// Gets videos for a user
        /// </summary>
        [HttpGet("user/{userId:guid}")]
        public async Task<ActionResult<IEnumerable<VideoResponse>>> GetUserVideos(
            Guid userId,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 10,
            CancellationToken cancellationToken = default)
        {
            try
            {
                _logger.LogInformation("Getting videos for user {UserId}, page {Page}, size {PageSize}", 
                    userId, page, pageSize);
                
                var result = await _getUserVideosUseCase.ExecuteAsync(userId, page, pageSize, cancellationToken);
                
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting videos for user {UserId}", userId);
                return StatusCode(500, new { error = "An error occurred while retrieving videos" });
            }
        }

        /// <summary>
        /// Gets video status
        /// </summary>
        [HttpGet("{id:guid}/status")]
        public async Task<ActionResult<VideoStatusResponse>> GetVideoStatus(
            Guid id,
            CancellationToken cancellationToken = default)
        {
            try
            {
                _logger.LogInformation("Getting status for video {VideoId}", id);
                
                var result = await _getVideoStatusUseCase.ExecuteAsync(id, cancellationToken);
                
                if (result == null)
                {
                    return NotFound(new { error = "Video not found" });
                }
                
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting video status {VideoId}", id);
                return StatusCode(500, new { error = "An error occurred while retrieving video status" });
            }
        }

        /// <summary>
        /// Deletes a video
        /// </summary>
        [HttpDelete("{id:guid}")]
        public async Task<ActionResult> DeleteVideo(
            Guid id,
            CancellationToken cancellationToken = default)
        {
            try
            {
                // TODO: Implement DeleteVideoUseCase
                _logger.LogInformation("Deleting video {VideoId}", id);
                
                // Placeholder - implement the actual use case
                await Task.CompletedTask;
                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting video {VideoId}", id);
                return StatusCode(500, new { error = "An error occurred while deleting the video" });
            }
        }

        [HttpPost("quick-create-from-text")]
        public async Task<ActionResult<VideoResponse>> QuickCreateVideoFromText(
            [FromBody] CreateVideoFromTextRequest request,
            CancellationToken cancellationToken)
        {
            try
            {
                _logger.LogInformation("Quick creating video from text for user {UserId}", request.UserId);

                // Override settings for speed optimization
                var optimizedRequest = new CreateVideoFromTextRequest
                {
                    UserId = request.UserId,
                    TextPrompt = request.TextPrompt,
                    Title = request.Title,
                    Description = request.Description,
                    Resolution = VideoResolution.SD_480p, // Force lower resolution for speed
                    AspectRatio = request.AspectRatio,
                    Style = request.Style,
                    Duration = Math.Min(request.Duration, 3), // Max 3 seconds for quick generation
                    NegativePrompt = request.NegativePrompt
                };

                var result = await _createVideoFromTextUseCase.ExecuteAsync(optimizedRequest, cancellationToken);

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error quick creating video from text");
                return StatusCode(500, new { message = "An error occurred while creating the video", error = ex.Message });
            }
        }
    }
} 