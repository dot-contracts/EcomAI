namespace EcomVideoAI.Domain.Entities
{
    public class ProductImage
    {
        public Guid Id { get; private set; }
        public Guid ProductDataId { get; private set; }
        public string Url { get; private set; } = string.Empty;
        public string? AltText { get; set; }
        public int? Width { get; set; }
        public int? Height { get; set; }
        public long? FileSizeBytes { get; set; }
        public bool IsPrimary { get; private set; } = false;
        public int SortOrder { get; set; } = 0;
        public DateTime CreatedAt { get; private set; }

        // Navigation Properties
        public virtual ProductData ProductData { get; set; } = null!;

        private ProductImage() { } // EF Core constructor

        public ProductImage(
            Guid productDataId,
            string url,
            string? altText = null,
            int? width = null,
            int? height = null,
            long? fileSizeBytes = null)
        {
            Id = Guid.NewGuid();
            ProductDataId = productDataId;
            Url = url ?? throw new ArgumentNullException(nameof(url));
            AltText = altText;
            Width = width;
            Height = height;
            FileSizeBytes = fileSizeBytes;
            CreatedAt = DateTime.UtcNow;
        }

        public void SetAsPrimary()
        {
            IsPrimary = true;
        }

        public void RemoveAsPrimary()
        {
            IsPrimary = false;
        }

        public void UpdateDimensions(int width, int height)
        {
            Width = width;
            Height = height;
        }

        public void UpdateFileSize(long fileSizeBytes)
        {
            FileSizeBytes = fileSizeBytes;
        }

        public string AspectRatio => Width.HasValue && Height.HasValue && Height > 0 
            ? $"{Width}:{Height}" 
            : "Unknown";

        public bool IsLandscape => Width.HasValue && Height.HasValue && Width > Height;
        public bool IsPortrait => Width.HasValue && Height.HasValue && Height > Width;
        public bool IsSquare => Width.HasValue && Height.HasValue && Width == Height;
    }
} 