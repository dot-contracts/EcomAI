using EcomVideoAI.Domain.Enums;

namespace EcomVideoAI.Domain.Entities
{
    public class Payment : AuditableEntity
    {
        public Guid Id { get; private set; }
        public Guid UserId { get; private set; }
        public Guid? UserSubscriptionId { get; set; }
        public Guid? PaymentMethodId { get; set; }
        public decimal Amount { get; private set; }
        public string Currency { get; set; } = "USD";
        public PaymentStatus Status { get; private set; }
        public string PaymentIntentId { get; private set; } = string.Empty;
        public string Provider { get; private set; } = string.Empty; // 'Stripe', 'PayPal'
        public decimal? ProviderFee { get; set; }
        public string? Description { get; set; }
        public string? FailureReason { get; set; }
        public decimal RefundedAmount { get; private set; } = 0;
        public Dictionary<string, object> Metadata { get; set; } = new Dictionary<string, object>();
        public DateTime? PaidAt { get; private set; }

        // Navigation Properties
        public virtual User User { get; set; } = null!;
        public virtual UserSubscription? UserSubscription { get; set; }
        public virtual PaymentMethod? PaymentMethod { get; set; }
        public virtual ICollection<Invoice> Invoices { get; private set; } = new List<Invoice>();

        private Payment() { } // EF Core constructor

        public Payment(
            Guid userId,
            decimal amount,
            string paymentIntentId,
            string provider,
            string? description = null,
            Guid? userSubscriptionId = null,
            Guid? paymentMethodId = null)
        {
            Id = Guid.NewGuid();
            UserId = userId;
            Amount = amount;
            PaymentIntentId = paymentIntentId ?? throw new ArgumentNullException(nameof(paymentIntentId));
            Provider = provider ?? throw new ArgumentNullException(nameof(provider));
            Description = description;
            UserSubscriptionId = userSubscriptionId;
            PaymentMethodId = paymentMethodId;
            Status = PaymentStatus.Pending;
            CreatedAt = DateTime.UtcNow;
        }

        public void MarkAsSucceeded()
        {
            Status = PaymentStatus.Succeeded;
            PaidAt = DateTime.UtcNow;
            UpdatedAt = DateTime.UtcNow;
        }

        public void MarkAsFailed(string? reason = null)
        {
            Status = PaymentStatus.Failed;
            FailureReason = reason;
            UpdatedAt = DateTime.UtcNow;
        }

        public void ProcessRefund(decimal refundAmount)
        {
            if (refundAmount > Amount - RefundedAmount)
                throw new InvalidOperationException("Refund amount cannot exceed remaining payment amount");

            RefundedAmount += refundAmount;
            
            if (RefundedAmount >= Amount)
                Status = PaymentStatus.Refunded;
            
            UpdatedAt = DateTime.UtcNow;
        }

        public void Cancel()
        {
            Status = PaymentStatus.Cancelled;
            UpdatedAt = DateTime.UtcNow;
        }

        public bool IsSuccessful => Status == PaymentStatus.Succeeded;
        public bool IsPending => Status == PaymentStatus.Pending;
        public bool HasFailed => Status == PaymentStatus.Failed;
        public decimal RemainingAmount => Amount - RefundedAmount;
    }
} 