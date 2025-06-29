using EcomVideoAI.Domain.Enums;

namespace EcomVideoAI.Application.DTOs.Requests.Video
{
    public class CreateVideoFromTextRequest
    {
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string TextPrompt { get; set; } = string.Empty;
        public string? NegativePrompt { get; set; }
        public string? Style { get; set; }
        public VideoResolution Resolution { get; set; } = VideoResolution.SD_480p;
        public string AspectRatio { get; set; } = "9:16"; // Default mobile format
        public int Duration { get; set; } = 5;
        public Guid UserId { get; set; }
    }
} 