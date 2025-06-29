namespace EcomVideoAI.Domain.Entities
{
    public class UserPreferences : AuditableEntity
    {
        public Guid Id { get; private set; }
        public Guid UserId { get; private set; }
        public string DefaultVideoResolution { get; set; } = "1080x1920";
        public string DefaultAspectRatio { get; set; } = "9:16";
        public int DefaultVideoDuration { get; set; } = 5;
        public string PreferredAiProvider { get; set; } = "Freepik";
        public bool AutoSaveToLibrary { get; set; } = true;
        public bool AutoGenerateThumbnails { get; set; } = true;
        public bool WatermarkEnabled { get; set; } = false;
        public string DefaultVideoQuality { get; set; } = "High";
        public string PreferredLanguage { get; set; } = "en-US";
        public string Timezone { get; set; } = "UTC";
        public string DateFormat { get; set; } = "MM/DD/YYYY";
        public string TimeFormat { get; set; } = "12h";
        public string Theme { get; set; } = "Dark";
        public bool EmailNotificationsEnabled { get; set; } = true;
        public bool PushNotificationsEnabled { get; set; } = true;
        public bool MarketingEmailsEnabled { get; set; } = false;
        public bool TwoFactorEnabled { get; set; } = false;

        // Navigation Properties
        public virtual User User { get; set; } = null!;

        private UserPreferences() { } // EF Core constructor

        public UserPreferences(Guid userId)
        {
            Id = Guid.NewGuid();
            UserId = userId;
            CreatedAt = DateTime.UtcNow;
        }

        public void UpdateVideoDefaults(string resolution, string aspectRatio, int duration)
        {
            DefaultVideoResolution = resolution;
            DefaultAspectRatio = aspectRatio;
            DefaultVideoDuration = duration;
            UpdatedAt = DateTime.UtcNow;
        }

        public void UpdateNotificationSettings(bool email, bool push, bool marketing)
        {
            EmailNotificationsEnabled = email;
            PushNotificationsEnabled = push;
            MarketingEmailsEnabled = marketing;
            UpdatedAt = DateTime.UtcNow;
        }

        public void UpdateTheme(string theme)
        {
            Theme = theme;
            UpdatedAt = DateTime.UtcNow;
        }

        public void EnableTwoFactor()
        {
            TwoFactorEnabled = true;
            UpdatedAt = DateTime.UtcNow;
        }

        public void DisableTwoFactor()
        {
            TwoFactorEnabled = false;
            UpdatedAt = DateTime.UtcNow;
        }
    }
} 