using EcomVideoAI.Domain.Enums;

namespace EcomVideoAI.Domain.Entities
{
    public class UserSubscription : AuditableEntity
    {
        public Guid Id { get; private set; }
        public Guid UserId { get; private set; }
        public Guid SubscriptionPlanId { get; private set; }
        public SubscriptionStatus Status { get; private set; }
        public BillingCycle BillingCycle { get; private set; }
        public decimal PricePaid { get; private set; }
        public string Currency { get; set; } = "USD";
        public DateTime StartedAt { get; private set; }
        public DateTime EndsAt { get; private set; }
        public DateTime? CancelledAt { get; private set; }
        public DateTime? TrialEndsAt { get; private set; }
        public bool AutoRenew { get; set; } = true;
        public Guid? PaymentMethodId { get; set; }
        public string? StripeSubscriptionId { get; set; }
        public Dictionary<string, object> Metadata { get; set; } = new Dictionary<string, object>();

        // Navigation Properties
        public virtual User User { get; set; } = null!;
        public virtual SubscriptionPlan SubscriptionPlan { get; set; } = null!;
        public virtual PaymentMethod? PaymentMethod { get; set; }
        public virtual ICollection<Payment> Payments { get; private set; } = new List<Payment>();
        public virtual ICollection<UsageCredit> UsageCredits { get; private set; } = new List<UsageCredit>();

        private UserSubscription() { } // EF Core constructor

        public UserSubscription(
            Guid userId,
            Guid subscriptionPlanId,
            BillingCycle billingCycle,
            decimal pricePaid,
            DateTime startsAt,
            DateTime endsAt,
            Guid? paymentMethodId = null)
        {
            Id = Guid.NewGuid();
            UserId = userId;
            SubscriptionPlanId = subscriptionPlanId;
            BillingCycle = billingCycle;
            PricePaid = pricePaid;
            StartedAt = startsAt;
            EndsAt = endsAt;
            PaymentMethodId = paymentMethodId;
            Status = SubscriptionStatus.Active;
            CreatedAt = DateTime.UtcNow;
        }

        public void Cancel()
        {
            Status = SubscriptionStatus.Cancelled;
            CancelledAt = DateTime.UtcNow;
            AutoRenew = false;
            UpdatedAt = DateTime.UtcNow;
        }

        public void Expire()
        {
            Status = SubscriptionStatus.Expired;
            UpdatedAt = DateTime.UtcNow;
        }

        public void Renew(DateTime newEndDate, decimal newPrice)
        {
            EndsAt = newEndDate;
            PricePaid = newPrice;
            Status = SubscriptionStatus.Active;
            UpdatedAt = DateTime.UtcNow;
        }

        public void StartTrial(DateTime trialEndDate)
        {
            Status = SubscriptionStatus.Trial;
            TrialEndsAt = trialEndDate;
            UpdatedAt = DateTime.UtcNow;
        }

        public void SetPaymentMethod(Guid paymentMethodId)
        {
            PaymentMethodId = paymentMethodId;
            UpdatedAt = DateTime.UtcNow;
        }

        public bool IsActive => Status == SubscriptionStatus.Active;
        public bool IsTrial => Status == SubscriptionStatus.Trial;
        public bool IsExpired => Status == SubscriptionStatus.Expired || EndsAt < DateTime.UtcNow;
        public bool IsCancelled => Status == SubscriptionStatus.Cancelled;

        public int DaysRemaining => (EndsAt - DateTime.UtcNow).Days;
        public bool IsNearExpiration => DaysRemaining <= 7 && DaysRemaining > 0;
    }
} 