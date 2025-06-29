using EcomVideoAI.Domain.Enums;

namespace EcomVideoAI.Application.DTOs.Responses.Video
{
    public class VideoResponse
    {
        public Guid Id { get; set; }
        public Guid UserId { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string TextPrompt { get; set; } = string.Empty;
        public VideoInputType InputType { get; set; }
        public VideoStatus Status { get; set; }
        public VideoResolution Resolution { get; set; }
        public int DurationSeconds { get; set; }
        public string? ImageUrl { get; set; }
        public string? VideoUrl { get; set; }
        public string? ThumbnailUrl { get; set; }
        public DateTime? CompletedAt { get; set; }
        public string? ErrorMessage { get; set; }
        public long FileSizeBytes { get; set; }
        public VideoMetadataResponse? Metadata { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }

    public class VideoMetadataResponse
    {
        public int Width { get; set; }
        public int Height { get; set; }
        public double FrameRate { get; set; }
        public string Format { get; set; } = string.Empty;
        public string Codec { get; set; } = string.Empty;
        public int Bitrate { get; set; }
        public string AspectRatio { get; set; } = string.Empty;
    }
} 