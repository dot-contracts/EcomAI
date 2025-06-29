using EcomVideoAI.Domain.Enums;

namespace EcomVideoAI.Domain.Interfaces.Services
{
    public interface IFreepikService
    {
        Task<FreepikImageGenerationResult> GenerateImageFromTextAsync(
            string prompt, 
            string? negativePrompt = null,
            string? style = null,
            VideoResolution resolution = VideoResolution.SD_480p,
            CancellationToken cancellationToken = default);

        Task<FreepikVideoGenerationResult> GenerateVideoFromImageAsync(
            string imageUrl,
            string? prompt = null,
            VideoResolution resolution = VideoResolution.SD_480p,
            int duration = 5,
            CancellationToken cancellationToken = default);

        Task<FreepikTaskStatus> GetImageTaskStatusAsync(
            string taskId,
            CancellationToken cancellationToken = default);

        Task<FreepikTaskStatus> GetVideoTaskStatusAsync(
            string taskId,
            CancellationToken cancellationToken = default);

        Task<FreepikImageResult?> GetImageResultAsync(
            string taskId,
            CancellationToken cancellationToken = default);

        Task<FreepikVideoResult?> GetVideoResultAsync(
            string taskId,
            CancellationToken cancellationToken = default);

        Task<FreepikTaskStatusResponse> GetImageGenerationStatus(string taskId, CancellationToken cancellationToken = default);
        Task<FreepikTaskStatusResponse> GetVideoGenerationStatus(string taskId, CancellationToken cancellationToken = default);
        Task<FreepikVideoGenerationResult> GenerateVideoFromImage(string imageId, CancellationToken cancellationToken = default);
    }

    public class FreepikImageGenerationResult
    {
        public string TaskId { get; set; } = string.Empty;
        public FreepikTaskStatusType Status { get; set; }
        public string? ErrorMessage { get; set; }
    }

    public class FreepikVideoGenerationResult
    {
        public string TaskId { get; set; } = string.Empty;
        public FreepikTaskStatusType Status { get; set; }
        public string? ErrorMessage { get; set; }
    }

    public class FreepikTaskStatus
    {
        public string TaskId { get; set; } = string.Empty;
        public FreepikTaskStatusType Status { get; set; }
        public string? ErrorMessage { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? CompletedAt { get; set; }
    }

    public class FreepikImageResult
    {
        public string TaskId { get; set; } = string.Empty;
        public string ImageUrl { get; set; } = string.Empty;
        public int Width { get; set; }
        public int Height { get; set; }
        public string Format { get; set; } = string.Empty;
        public long FileSizeBytes { get; set; }
    }

    public class FreepikVideoResult
    {
        public string TaskId { get; set; } = string.Empty;
        public string VideoUrl { get; set; } = string.Empty;
        public int Width { get; set; }
        public int Height { get; set; }
        public double Duration { get; set; }
        public string Format { get; set; } = string.Empty;
        public long FileSizeBytes { get; set; }
        public string? ThumbnailUrl { get; set; }
    }

    public class FreepikTaskStatusResponse
    {
        public string Status { get; set; }
        public FreepikDataItem[] Data { get; set; }
    }

    public class FreepikDataItem
    {
        public Guid Id { get; set; }
        public string Url { get; set; }
        public FreepikLink[] Links { get; set; }
    }

    public class FreepikLink
    {
        public string Rel { get; set; }
        public string Href { get; set; }
    }

    public enum FreepikTaskStatusType
    {
        InProgress,
        Completed,
        Failed,
        Cancelled
    }
} 