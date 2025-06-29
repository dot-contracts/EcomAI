namespace EcomVideoAI.Domain.Entities
{
    public class NotificationSettings : AuditableEntity
    {
        public Guid Id { get; private set; }
        public Guid UserId { get; private set; }
        public bool VideoCompleted { get; set; } = true;
        public bool VideoFailed { get; set; } = true;
        public bool SubscriptionRenewal { get; set; } = true;
        public bool SubscriptionExpired { get; set; } = true;
        public bool PaymentFailed { get; set; } = true;
        public bool NewFeatures { get; set; } = true;
        public bool MarketingUpdates { get; set; } = false;
        public bool WeeklyDigest { get; set; } = true;
        public bool EmailEnabled { get; set; } = true;
        public bool PushEnabled { get; set; } = true;
        public bool SmsEnabled { get; set; } = false;

        // Navigation Properties
        public virtual User User { get; set; } = null!;

        private NotificationSettings() { } // EF Core constructor

        public NotificationSettings(Guid userId)
        {
            Id = Guid.NewGuid();
            UserId = userId;
            CreatedAt = DateTime.UtcNow;
        }

        public void UpdateEmailSettings(bool enabled)
        {
            EmailEnabled = enabled;
            UpdatedAt = DateTime.UtcNow;
        }

        public void UpdatePushSettings(bool enabled)
        {
            PushEnabled = enabled;
            UpdatedAt = DateTime.UtcNow;
        }

        public void UpdateSmsSettings(bool enabled)
        {
            SmsEnabled = enabled;
            UpdatedAt = DateTime.UtcNow;
        }

        public void DisableAllNotifications()
        {
            VideoCompleted = false;
            VideoFailed = false;
            SubscriptionRenewal = false;
            SubscriptionExpired = false;
            PaymentFailed = false;
            NewFeatures = false;
            MarketingUpdates = false;
            WeeklyDigest = false;
            UpdatedAt = DateTime.UtcNow;
        }

        public void EnableEssentialNotifications()
        {
            VideoCompleted = true;
            VideoFailed = true;
            SubscriptionExpired = true;
            PaymentFailed = true;
            UpdatedAt = DateTime.UtcNow;
        }
    }
} 