using EcomVideoAI.Domain.Enums;

namespace EcomVideoAI.Domain.Entities
{
    public class PaymentMethod : AuditableEntity
    {
        public Guid Id { get; private set; }
        public Guid UserId { get; private set; }
        public PaymentMethodType Type { get; private set; }
        public string Provider { get; private set; } = string.Empty; // 'Stripe', 'PayPal'
        public string ExternalId { get; private set; } = string.Empty; // Provider's payment method ID
        public string? LastFourDigits { get; set; }
        public string? Brand { get; set; } // 'Visa', 'MasterCard', etc.
        public DateTime? ExpiresAt { get; set; }
        public bool IsDefault { get; private set; } = false;
        public bool IsActive { get; private set; } = true;
        public Dictionary<string, object> BillingAddress { get; set; } = new Dictionary<string, object>();
        public Dictionary<string, object> Metadata { get; set; } = new Dictionary<string, object>();

        // Navigation Properties
        public virtual User User { get; set; } = null!;
        public virtual ICollection<UserSubscription> Subscriptions { get; private set; } = new List<UserSubscription>();
        public virtual ICollection<Payment> Payments { get; private set; } = new List<Payment>();

        private PaymentMethod() { } // EF Core constructor

        public PaymentMethod(
            Guid userId,
            PaymentMethodType type,
            string provider,
            string externalId,
            string? lastFourDigits = null,
            string? brand = null)
        {
            Id = Guid.NewGuid();
            UserId = userId;
            Type = type;
            Provider = provider ?? throw new ArgumentNullException(nameof(provider));
            ExternalId = externalId ?? throw new ArgumentNullException(nameof(externalId));
            LastFourDigits = lastFourDigits;
            Brand = brand;
            CreatedAt = DateTime.UtcNow;
        }

        public void SetAsDefault()
        {
            IsDefault = true;
            UpdatedAt = DateTime.UtcNow;
        }

        public void RemoveAsDefault()
        {
            IsDefault = false;
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

        public void UpdateExpiration(DateTime expirationDate)
        {
            ExpiresAt = expirationDate;
            UpdatedAt = DateTime.UtcNow;
        }

        public bool IsExpired => ExpiresAt.HasValue && ExpiresAt < DateTime.UtcNow;
        public string MaskedNumber => LastFourDigits != null ? $"****{LastFourDigits}" : "****";
    }
} 