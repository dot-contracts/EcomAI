namespace EcomVideoAI.Domain.Entities
{
    public class SubscriptionPlan : AuditableEntity
    {
        public Guid Id { get; private set; }
        public required string Name { get; set; }
        public string? Description { get; set; }
        public decimal PriceMonthly { get; set; }
        public decimal? PriceYearly { get; set; }
        public string Currency { get; set; } = "USD";
        public int VideoCreditsMonthly { get; set; } = 0;
        public int MaxVideoDuration { get; set; } = 15; // seconds
        public string MaxResolution { get; set; } = "1080p";
        public List<string> AiProviders { get; set; } = new List<string>();
        public List<string> Features { get; set; } = new List<string>();
        public bool IsPopular { get; set; } = false;
        public bool IsActive { get; set; } = true;
        public int TrialDays { get; set; } = 0;
        public int SortOrder { get; set; } = 0;

        // Navigation Properties
        public virtual ICollection<UserSubscription> UserSubscriptions { get; private set; } = new List<UserSubscription>();

        private SubscriptionPlan() { } // EF Core constructor

        public SubscriptionPlan(string name, decimal priceMonthly, int videoCreditsMonthly)
        {
            Id = Guid.NewGuid();
            Name = name ?? throw new ArgumentNullException(nameof(name));
            PriceMonthly = priceMonthly;
            VideoCreditsMonthly = videoCreditsMonthly;
            CreatedAt = DateTime.UtcNow;
        }

        public void UpdatePricing(decimal monthlyPrice, decimal? yearlyPrice = null)
        {
            PriceMonthly = monthlyPrice;
            PriceYearly = yearlyPrice;
            UpdatedAt = DateTime.UtcNow;
        }

        public void AddFeature(string feature)
        {
            if (!Features.Contains(feature))
            {
                Features.Add(feature);
                UpdatedAt = DateTime.UtcNow;
            }
        }

        public void RemoveFeature(string feature)
        {
            if (Features.Remove(feature))
            {
                UpdatedAt = DateTime.UtcNow;
            }
        }

        public void SetPopular(bool isPopular)
        {
            IsPopular = isPopular;
            UpdatedAt = DateTime.UtcNow;
        }

        public void Activate()
        {
            IsActive = true;
            UpdatedAt = DateTime.UtcNow;
        }

        public void Deactivate()
        {
            IsActive = false;
            UpdatedAt = DateTime.UtcNow;
        }

        public decimal GetYearlyDiscount()
        {
            if (PriceYearly.HasValue)
            {
                var yearlyEquivalent = PriceMonthly * 12;
                return yearlyEquivalent - PriceYearly.Value;
            }
            return 0;
        }

        public bool HasFeature(string feature) => Features.Contains(feature);
    }
} 