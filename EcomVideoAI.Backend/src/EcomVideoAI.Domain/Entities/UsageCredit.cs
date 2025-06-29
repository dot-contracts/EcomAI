using EcomVideoAI.Domain.Enums;

namespace EcomVideoAI.Domain.Entities
{
    public class UsageCredit : AuditableEntity
    {
        public Guid Id { get; private set; }
        public Guid UserId { get; private set; }
        public Guid? UserSubscriptionId { get; set; }
        public CreditType CreditType { get; private set; }
        public int Amount { get; private set; }
        public int UsedAmount { get; private set; } = 0;
        public CreditSource Source { get; private set; }
        public DateTime? ExpiresAt { get; set; }
        public bool IsActive { get; private set; } = true;
        public Dictionary<string, object> Metadata { get; set; } = new Dictionary<string, object>();

        // Navigation Properties
        public virtual User User { get; set; } = null!;
        public virtual UserSubscription? UserSubscription { get; set; }

        private UsageCredit() { } // EF Core constructor

        public UsageCredit(
            Guid userId,
            CreditType creditType,
            int amount,
            CreditSource source,
            DateTime? expiresAt = null,
            Guid? userSubscriptionId = null)
        {
            Id = Guid.NewGuid();
            UserId = userId;
            CreditType = creditType;
            Amount = amount;
            Source = source;
            ExpiresAt = expiresAt;
            UserSubscriptionId = userSubscriptionId;
            CreatedAt = DateTime.UtcNow;
        }

        public bool UseCredits(int creditsToUse)
        {
            if (creditsToUse <= 0)
                throw new ArgumentException("Credits to use must be positive", nameof(creditsToUse));

            if (RemainingCredits < creditsToUse)
                return false;

            if (IsExpired || !IsActive)
                return false;

            UsedAmount += creditsToUse;
            UpdatedAt = DateTime.UtcNow;
            return true;
        }

        public void Deactivate()
        {
            IsActive = false;
            UpdatedAt = DateTime.UtcNow;
        }

        public void Activate()
        {
            IsActive = true;
            UpdatedAt = DateTime.UtcNow;
        }

        public int RemainingCredits => Amount - UsedAmount;
        public bool IsExpired => ExpiresAt.HasValue && ExpiresAt < DateTime.UtcNow;
        public bool IsFullyUsed => UsedAmount >= Amount;
        public bool IsAvailable => IsActive && !IsExpired && !IsFullyUsed;
        public double UsagePercentage => Amount > 0 ? (double)UsedAmount / Amount * 100 : 0;
    }
} 