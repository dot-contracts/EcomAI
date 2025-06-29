using EcomVideoAI.Domain.Enums;

namespace EcomVideoAI.Domain.Entities
{
    public class ProductData : AuditableEntity
    {
        public Guid Id { get; private set; }
        public Guid UserId { get; private set; }
        public Guid? CategoryId { get; set; }
        public string OriginalUrl { get; private set; } = string.Empty;
        public string? Domain { get; set; }
        public string? Platform { get; set; } // 'Shopify', 'Amazon', 'Etsy', etc.
        public string? ProductId { get; set; }
        public required string Name { get; set; }
        public string? Description { get; set; }
        public decimal? Price { get; set; }
        public string Currency { get; set; } = "USD";
        public decimal? OriginalPrice { get; set; }
        public int? DiscountPercent { get; set; }
        public string? Brand { get; set; }
        public string? Sku { get; set; }
        public string? Availability { get; set; }
        public decimal? Rating { get; set; }
        public int ReviewCount { get; set; } = 0;
        public string? MainImageUrl { get; set; }
        public ScrapingStatus ScrapingStatus { get; private set; } = ScrapingStatus.Pending;
        public DateTime? ScrapedAt { get; private set; }
        public string? ErrorMessage { get; set; }
        public Dictionary<string, object> Metadata { get; set; } = new Dictionary<string, object>();

        // Navigation Properties
        public virtual User User { get; set; } = null!;
        public virtual ProductCategory? Category { get; set; }
        public virtual ICollection<ProductImage> Images { get; private set; } = new List<ProductImage>();
        public virtual ICollection<Video> Videos { get; private set; } = new List<Video>();

        private ProductData() { } // EF Core constructor

        public ProductData(Guid userId, string originalUrl, string name)
        {
            Id = Guid.NewGuid();
            UserId = userId;
            OriginalUrl = originalUrl ?? throw new ArgumentNullException(nameof(originalUrl));
            Name = name ?? throw new ArgumentNullException(nameof(name));
            CreatedAt = DateTime.UtcNow;
        }

        public void MarkAsScraped()
        {
            ScrapingStatus = ScrapingStatus.Success;
            ScrapedAt = DateTime.UtcNow;
            UpdatedAt = DateTime.UtcNow;
        }

        public void MarkAsFailed(string errorMessage)
        {
            ScrapingStatus = ScrapingStatus.Failed;
            ErrorMessage = errorMessage;
            UpdatedAt = DateTime.UtcNow;
        }

        public void UpdatePricing(decimal price, decimal? originalPrice = null)
        {
            Price = price;
            OriginalPrice = originalPrice;
            DiscountPercent = originalPrice.HasValue && originalPrice > 0 
                ? (int)Math.Round(((originalPrice.Value - price) / originalPrice.Value) * 100)
                : null;
            UpdatedAt = DateTime.UtcNow;
        }

        public void UpdateRating(decimal rating, int reviewCount)
        {
            Rating = rating;
            ReviewCount = reviewCount;
            UpdatedAt = DateTime.UtcNow;
        }

        public bool HasDiscount => DiscountPercent.HasValue && DiscountPercent > 0;
        public decimal? SavingsAmount => OriginalPrice.HasValue && Price.HasValue 
            ? OriginalPrice.Value - Price.Value 
            : null;
    }
} 