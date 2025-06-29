using EcomVideoAI.Domain.Enums;
using EcomVideoAI.Domain.Interfaces.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text;

namespace EcomVideoAI.Infrastructure.ExternalServices
{
    public class FreepikService : IFreepikService
    {
        private readonly HttpClient _httpClient;
        private readonly FreepikOptions _options;
        private readonly ILogger<FreepikService> _logger;

        public FreepikService(
            HttpClient httpClient,
            IOptions<FreepikOptions> options,
            ILogger<FreepikService> logger)
        {
            _httpClient = httpClient;
            _options = options.Value;
            _logger = logger;

            // Configure HttpClient
            _httpClient.BaseAddress = new Uri(_options.BaseUrl);
            _httpClient.DefaultRequestHeaders.Add("x-freepik-api-key", _options.ApiKey);
        }

        public async Task<FreepikImageGenerationResult> GenerateImageFromTextAsync(
            string prompt, 
            string? negativePrompt = null,
            string? style = null,
            VideoResolution resolution = VideoResolution.SD_480p,
            CancellationToken cancellationToken = default)
        {
            try
            {
                _logger.LogInformation("Starting image generation for prompt: {Prompt}", prompt);

                var requestBody = new
                {
                    prompt = prompt,
                    aspect_ratio = "square_1_1",
                    model = "realism",
                    num_images = 1,
                    resolution = "1k",
                    engine = "automatic",
                    creative_detailing = 25,
                    filter_nsfw = true
                };

                var json = JsonSerializer.Serialize(requestBody);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await _httpClient.PostAsync("/v1/ai/mystic", content, cancellationToken);
                var responseContent = await response.Content.ReadAsStringAsync(cancellationToken);

                _logger.LogInformation("Freepik API response: {StatusCode}, Content: {Content}", 
                    response.StatusCode, responseContent);

                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogError("Freepik image generation failed: {StatusCode} - {Content}", 
                        response.StatusCode, responseContent);
                    return new FreepikImageGenerationResult
                    {
                        Status = FreepikTaskStatusType.Failed,
                        ErrorMessage = $"API call failed: {response.StatusCode}"
                    };
                }

                var options = new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true,
                    PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower
                };

                var result = JsonSerializer.Deserialize<FreepikMysticResponse>(responseContent, options);

                if (result != null && !string.IsNullOrEmpty(result.Data.TaskId))
                {
                    _logger.LogInformation("Image generation initiated with task ID: {TaskId}", result.Data.TaskId);
                    
                    return new FreepikImageGenerationResult
                    {
                        TaskId = result.Data.TaskId,
                        Status = MapStatus(result.Data.Status)
                    };
                }

                _logger.LogError("Failed to parse response or missing task ID");
                return new FreepikImageGenerationResult
                {
                    Status = FreepikTaskStatusType.Failed,
                    ErrorMessage = "Failed to start image generation - no task ID received"
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Exception occurred during image generation");
                return new FreepikImageGenerationResult
                {
                    Status = FreepikTaskStatusType.Failed,
                    ErrorMessage = ex.Message
                };
            }
        }

        public async Task<FreepikVideoGenerationResult> GenerateVideoFromImageAsync(
            string imageUrl, 
            string? prompt = null, 
            VideoResolution resolution = VideoResolution.SD_480p, 
            int duration = 3,
            CancellationToken cancellationToken = default)
        {
            try
            {
                _logger.LogInformation("Generating video from image: {ImageUrl}", imageUrl);

                // Freepik API only accepts duration values of "5" or "10"
                var validDuration = duration <= 5 ? "5" : "10";
                _logger.LogInformation("Requested duration: {RequestedDuration}, using valid duration: {ValidDuration}", duration, validDuration);

                var request = new
                {
                    image = imageUrl,
                    duration = validDuration,
                    // Note: Based on Freepik API docs, resolution is handled differently
                    // You may need to adjust this based on actual API requirements
                };

                // Using Kling v2 endpoint as shown in the search results
                var response = await _httpClient.PostAsJsonAsync("/v1/ai/image-to-video/kling-v2", request, cancellationToken);
                
                if (!response.IsSuccessStatusCode)
                {
                    var errorContent = await response.Content.ReadAsStringAsync(cancellationToken);
                    _logger.LogError("Freepik video generation failed: {StatusCode} - {Error}", response.StatusCode, errorContent);
                    
                    return new FreepikVideoGenerationResult
                    {
                        Status = FreepikTaskStatusType.Failed,
                        ErrorMessage = $"API call failed: {response.StatusCode} - {errorContent}"
                    };
                }

                var jsonResponse = await response.Content.ReadAsStringAsync(cancellationToken);
                _logger.LogInformation("Video generation API response: {Response}", jsonResponse);

                // Try to deserialize as FreepikMysticResponse first (same format as image generation)
                var options = new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true,
                    PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower
                };

                var mysticResult = JsonSerializer.Deserialize<FreepikMysticResponse>(jsonResponse, options);

                if (mysticResult?.Data != null && !string.IsNullOrEmpty(mysticResult.Data.TaskId))
                {
                    _logger.LogInformation("Video generation initiated with task ID: {TaskId}", mysticResult.Data.TaskId);
                    
                    return new FreepikVideoGenerationResult
                    {
                        TaskId = mysticResult.Data.TaskId,
                        Status = MapStatus(mysticResult.Data.Status)
                    };
                }

                // Fallback: try the original FreepikVideoApiResponse format
                var result = JsonSerializer.Deserialize<FreepikVideoApiResponse>(jsonResponse, options);

                if (result?.Data != null && !string.IsNullOrEmpty(result.Data.Id))
                {
                    _logger.LogInformation("Video generation initiated with task ID: {TaskId}", result.Data.Id);
                    
                    return new FreepikVideoGenerationResult
                    {
                        TaskId = result.Data.Id,
                        Status = MapStatus(result.Data.Status)
                    };
                }

                _logger.LogError("Failed to parse video generation response. Response: {Response}", jsonResponse);
                return new FreepikVideoGenerationResult
                {
                    Status = FreepikTaskStatusType.Failed,
                    ErrorMessage = "Invalid response from Freepik API - could not extract task ID"
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating video from image");
                return new FreepikVideoGenerationResult
                {
                    Status = FreepikTaskStatusType.Failed,
                    ErrorMessage = ex.Message
                };
            }
        }

        public async Task<FreepikTaskStatus> GetImageTaskStatusAsync(string taskId, CancellationToken cancellationToken = default)
        {
            try
            {
                var response = await _httpClient.GetAsync($"/v1/ai/mystic/{taskId}", cancellationToken);
                return await ProcessTaskStatusResponse(response, taskId, cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting image task status for {TaskId}", taskId);
                return new FreepikTaskStatus
                {
                    TaskId = taskId,
                    Status = FreepikTaskStatusType.Failed,
                    ErrorMessage = ex.Message
                };
            }
        }

        public async Task<FreepikTaskStatus> GetVideoTaskStatusAsync(string taskId, CancellationToken cancellationToken = default)
        {
            try
            {
                var response = await _httpClient.GetAsync($"/v1/ai/image-to-video/kling-v2/{taskId}", cancellationToken);
                return await ProcessTaskStatusResponse(response, taskId, cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting video task status for {TaskId}", taskId);
                return new FreepikTaskStatus
                {
                    TaskId = taskId,
                    Status = FreepikTaskStatusType.Failed,
                    ErrorMessage = ex.Message
                };
            }
        }

        public async Task<FreepikImageResult?> GetImageResultAsync(string taskId, CancellationToken cancellationToken = default)
        {
            try
            {
                var response = await _httpClient.GetAsync($"/v1/ai/mystic/{taskId}", cancellationToken);
                
                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogError("Failed to get image result for task {TaskId}: {StatusCode}", taskId, response.StatusCode);
                    return null;
                }

                var jsonResponse = await response.Content.ReadAsStringAsync(cancellationToken);
                _logger.LogInformation("Image result response for task {TaskId}: {Response}", taskId, jsonResponse);

                var options = new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true,
                    PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower
                };

                var result = JsonSerializer.Deserialize<FreepikMysticResponse>(jsonResponse, options);

                if (result?.Data != null && result.Data.Status?.ToUpperInvariant() == "COMPLETED" && result.Data.Generated?.Length > 0)
                {
                    var imageUrl = result.Data.Generated[0];
                    _logger.LogInformation("Successfully retrieved image URL for task {TaskId}: {ImageUrl}", taskId, imageUrl);
                    
                    return new FreepikImageResult
                    {
                        TaskId = taskId,
                        ImageUrl = imageUrl,
                        Width = 1024,  // Default values - adjust based on actual API response if available
                        Height = 1024,
                        Format = "png",
                        FileSizeBytes = 0
                    };
                }

                _logger.LogWarning("Image task {TaskId} not completed or no generated images. Status: {Status}, Generated count: {Count}", 
                    taskId, result?.Data?.Status, result?.Data?.Generated?.Length ?? 0);
                return null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting image result for task {TaskId}", taskId);
                return null;
            }
        }

        public async Task<FreepikVideoResult?> GetVideoResultAsync(string taskId, CancellationToken cancellationToken = default)
        {
            try
            {
                var response = await _httpClient.GetAsync($"/v1/ai/image-to-video/kling-v2/{taskId}", cancellationToken);
                
                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogError("Failed to get video result for task {TaskId}: {StatusCode}", taskId, response.StatusCode);
                    return null;
                }

                var jsonResponse = await response.Content.ReadAsStringAsync(cancellationToken);
                _logger.LogInformation("Video result response for task {TaskId}: {Response}", taskId, jsonResponse);

                var options = new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true,
                    PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower
                };

                var result = JsonSerializer.Deserialize<FreepikMysticResponse>(jsonResponse, options);

                if (result?.Data != null && result.Data.Status?.ToUpperInvariant() == "COMPLETED" && result.Data.Generated?.Length > 0)
                {
                    var videoUrl = result.Data.Generated[0];
                    _logger.LogInformation("Successfully retrieved video URL for task {TaskId}: {VideoUrl}", taskId, videoUrl);
                    
                    return new FreepikVideoResult
                    {
                        TaskId = taskId,
                        VideoUrl = videoUrl,
                        Width = 1024,  // Default values - adjust based on actual API response if available
                        Height = 1024,
                        Duration = 5.0,
                        Format = "mp4",
                        FileSizeBytes = 0,
                        ThumbnailUrl = null
                    };
                }

                _logger.LogWarning("Video task {TaskId} not completed or no generated videos. Status: {Status}, Generated count: {Count}", 
                    taskId, result?.Data?.Status, result?.Data?.Generated?.Length ?? 0);
                return null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting video result for task {TaskId}", taskId);
                return null;
            }
        }

        public async Task<FreepikTaskStatusResponse> GetImageGenerationStatus(string taskId, CancellationToken cancellationToken = default)
        {
            var response = await _httpClient.GetAsync($"/v1/ai/mystic/{taskId}", cancellationToken);
            response.EnsureSuccessStatusCode();
            var json = await response.Content.ReadAsStringAsync(cancellationToken);
            return JsonSerializer.Deserialize<FreepikTaskStatusResponse>(json, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
        }

        public async Task<FreepikTaskStatusResponse> GetVideoGenerationStatus(string taskId, CancellationToken cancellationToken = default)
        {
            var response = await _httpClient.GetAsync($"/v1/ai/image-to-video/kling-v2/{taskId}", cancellationToken);
            response.EnsureSuccessStatusCode();
            var json = await response.Content.ReadAsStringAsync(cancellationToken);
            return JsonSerializer.Deserialize<FreepikTaskStatusResponse>(json, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
        }

        public async Task<FreepikVideoGenerationResult> GenerateVideoFromImage(string imageId, CancellationToken cancellationToken = default)
        {
             var request = new
            {
                image = imageId,
                duration = "5",
            };
            var response = await _httpClient.PostAsJsonAsync("/v1/ai/image-to-video/kling-v2", request, cancellationToken);
            response.EnsureSuccessStatusCode();
            
            var jsonResponse = await response.Content.ReadAsStringAsync(cancellationToken);
            _logger.LogInformation("Video generation response: {Response}", jsonResponse);
            
            var result = JsonSerializer.Deserialize<FreepikMysticResponse>(jsonResponse, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true,
                PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower
            });

            if (result?.Data?.TaskId == null)
            {
                _logger.LogError("Failed to extract task ID from video generation response: {Response}", jsonResponse);
                throw new InvalidOperationException("Failed to extract task ID from video generation response");
            }

            _logger.LogInformation("Successfully started video generation with task ID: {TaskId}", result.Data.TaskId);

            return new FreepikVideoGenerationResult
            {
                TaskId = result.Data.TaskId,
                Status = MapStatus(result.Data.Status)
            };
        }

        private async Task<FreepikTaskStatus> ProcessTaskStatusResponse(HttpResponseMessage response, string taskId, CancellationToken cancellationToken)
        {
            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync(cancellationToken);
                _logger.LogError("Task status API call failed for {TaskId}: {StatusCode} - {Error}", taskId, response.StatusCode, errorContent);
                return new FreepikTaskStatus
                {
                    TaskId = taskId,
                    Status = FreepikTaskStatusType.Failed,
                    ErrorMessage = $"API call failed: {response.StatusCode} - {errorContent}"
                };
            }

            var jsonResponse = await response.Content.ReadAsStringAsync(cancellationToken);
            _logger.LogInformation("Task status API response for {TaskId}: {Response}", taskId, jsonResponse);

            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true,
                PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower
            };

            // Use FreepikMysticResponse format (same as image/video result methods)
            var result = JsonSerializer.Deserialize<FreepikMysticResponse>(jsonResponse, options);

            if (result?.Data != null)
            {
                var status = MapStatus(result.Data.Status);
                _logger.LogInformation("Parsed task status for {TaskId}: {Status}, Generated count: {GeneratedCount}", 
                    taskId, status, result.Data.Generated?.Length ?? 0);

                return new FreepikTaskStatus
                {
                    TaskId = taskId,
                    Status = status,
                    ErrorMessage = status == FreepikTaskStatusType.Failed ? "Task failed" : null,
                    CreatedAt = DateTime.UtcNow,
                    CompletedAt = status == FreepikTaskStatusType.Completed ? DateTime.UtcNow : null
                };
            }

            _logger.LogError("Failed to parse task status response for {TaskId}: {Response}", taskId, jsonResponse);
            return new FreepikTaskStatus
            {
                TaskId = taskId,
                Status = FreepikTaskStatusType.Failed,
                ErrorMessage = "Failed to parse API response",
                CreatedAt = DateTime.UtcNow
            };
        }

        private static FreepikTaskStatusType MapStatus(string? status)
        {
            return status?.ToUpperInvariant() switch
            {
                "IN_PROGRESS" => FreepikTaskStatusType.InProgress,
                "COMPLETED" => FreepikTaskStatusType.Completed,
                "FAILED" => FreepikTaskStatusType.Failed,
                "CANCELLED" => FreepikTaskStatusType.Cancelled,
                _ => FreepikTaskStatusType.InProgress
            };
        }

        private static string GetFreepikImageSize(VideoResolution resolution)
        {
            return resolution switch
            {
                VideoResolution.SD_480p => "square",
                VideoResolution.HD_720p => "square_hd",
                VideoResolution.FullHD_1080p => "square_hd",
                _ => "square"
            };
        }

        private static string GetAspectRatioFromResolution(VideoResolution resolution)
        {
            return resolution switch
            {
                VideoResolution.HD_720p => "widescreen_16_9",
                VideoResolution.FullHD_1080p => "widescreen_16_9",
                VideoResolution.SD_480p => "widescreen_16_9",
                _ => "widescreen_16_9"
            };
        }
    }

    // API Response Models
    public class FreepikImageApiResponse
    {
        public FreepikTaskData[]? Data { get; set; }
    }

    public class FreepikVideoApiResponse
    {
        public FreepikTaskData? Data { get; set; }
    }

    public class FreepikTaskStatusApiResponse
    {
        public FreepikTaskStatusData? Data { get; set; }
    }

    public class FreepikTaskData
    {
        public string Id { get; set; } = string.Empty;
        public string? Status { get; set; }
    }

    public class FreepikTaskStatusData
    {
        public string TaskId { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public string? ErrorMessage { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? CompletedAt { get; set; }
    }

    public class FreepikMysticResponse
    {
        public FreepikMysticData Data { get; set; } = new();
    }

    public class FreepikMysticData
    {
        public string TaskId { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public string[]? Generated { get; set; }
    }

    // Configuration Options
    public class FreepikOptions
    {
        public string ApiKey { get; set; } = string.Empty;
        public string BaseUrl { get; set; } = "https://api.freepik.com";
    }
} 