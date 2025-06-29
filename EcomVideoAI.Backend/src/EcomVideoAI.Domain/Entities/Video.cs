using EcomVideoAI.Domain.Enums;
using EcomVideoAI.Domain.ValueObjects;

namespace EcomVideoAI.Domain.Entities
{
    public class Video : AuditableEntity
    {
        public Guid Id { get; private set; }
        public Guid UserId { get; private set; }
        public required string Title { get; set; }
        public required string Description { get; set; }
        public required string TextPrompt { get; set; }
        public VideoInputType InputType { get; private set; }
        public VideoStatus Status { get; private set; }
        public VideoResolution Resolution { get; private set; }
        public AspectRatio AspectRatio { get; private set; }
        public string? Style { get; private set; }
        public int DurationSeconds { get; private set; }
        public string? ImageUrl { get; private set; }
        public string? VideoUrl { get; private set; }
        public string? ThumbnailUrl { get; private set; }
        public string? FreepikTaskId { get; private set; }
        public string? FreepikImageTaskId { get; private set; }
        public DateTime? CompletedAt { get; private set; }
        public string? ErrorMessage { get; private set; }
        public long FileSizeBytes { get; private set; }
        public VideoMetadata? Metadata { get; private set; }

        private Video() { } // EF Core constructor

        public Video(
            Guid userId,
            string title,
            string description,
            string textPrompt,
            VideoInputType inputType,
            VideoResolution resolution,
            AspectRatio aspectRatio,
            string? style = null,
            int durationSeconds = 5)
        {
            Id = Guid.NewGuid();
            UserId = userId;
            Title = title ?? throw new ArgumentNullException(nameof(title));
            Description = description ?? string.Empty;
            TextPrompt = textPrompt ?? throw new ArgumentNullException(nameof(textPrompt));
            InputType = inputType;
            Status = VideoStatus.Pending;
            Resolution = resolution;
            AspectRatio = aspectRatio;
            Style = style;
            DurationSeconds = durationSeconds;
            CreatedAt = DateTime.UtcNow;
        }

        public void UpdateStatus(VideoStatus status)
        {
            Status = status;
            UpdatedAt = DateTime.UtcNow;

            if (status == VideoStatus.Completed)
            {
                CompletedAt = DateTime.UtcNow;
            }
        }

        public void SetFreepikImageTaskId(string taskId)
        {
            FreepikImageTaskId = taskId ?? throw new ArgumentNullException(nameof(taskId));
            UpdatedAt = DateTime.UtcNow;
        }

        public void ClearFreepikImageTaskId()
        {
            FreepikImageTaskId = null;
            UpdatedAt = DateTime.UtcNow;
        }

        public void SetFreepikVideoTaskId(string taskId)
        {
            FreepikTaskId = taskId ?? throw new ArgumentNullException(nameof(taskId));
            UpdatedAt = DateTime.UtcNow;
        }

        public void SetImageUrl(string imageUrl)
        {
            ImageUrl = imageUrl ?? throw new ArgumentNullException(nameof(imageUrl));
            UpdatedAt = DateTime.UtcNow;
        }

        public void SetVideoUrl(string videoUrl, long fileSizeBytes)
        {
            VideoUrl = videoUrl ?? throw new ArgumentNullException(nameof(videoUrl));
            FileSizeBytes = fileSizeBytes;
            Status = VideoStatus.Completed;
            CompletedAt = DateTime.UtcNow;
            UpdatedAt = DateTime.UtcNow;
        }

        public void SetThumbnailUrl(string thumbnailUrl)
        {
            ThumbnailUrl = thumbnailUrl ?? throw new ArgumentNullException(nameof(thumbnailUrl));
            UpdatedAt = DateTime.UtcNow;
        }

        public void SetError(string errorMessage)
        {
            ErrorMessage = errorMessage ?? throw new ArgumentNullException(nameof(errorMessage));
            Status = VideoStatus.Failed;
            UpdatedAt = DateTime.UtcNow;
        }

        public void SetMetadata(VideoMetadata metadata)
        {
            Metadata = metadata ?? throw new ArgumentNullException(nameof(metadata));
            UpdatedAt = DateTime.UtcNow;
        }

        public bool IsProcessing => Status == VideoStatus.Processing || Status == VideoStatus.GeneratingImage || Status == VideoStatus.GeneratingVideo;
        public bool IsCompleted => Status == VideoStatus.Completed;
        public bool HasFailed => Status == VideoStatus.Failed;
    }
} 