namespace EcomVideoAI.Domain.Entities
{
    public class EmailVerificationToken
    {
        public Guid Id { get; private set; }
        public Guid UserId { get; private set; }
        public string Token { get; private set; } = string.Empty;
        public DateTime ExpiresAt { get; private set; }
        public DateTime? UsedAt { get; private set; }
        public DateTime CreatedAt { get; private set; }

        // Navigation Properties
        public virtual User User { get; set; } = null!;

        private EmailVerificationToken() { } // EF Core constructor

        public EmailVerificationToken(Guid userId, string token, DateTime expiresAt)
        {
            Id = Guid.NewGuid();
            UserId = userId;
            Token = token ?? throw new ArgumentNullException(nameof(token));
            ExpiresAt = expiresAt;
            CreatedAt = DateTime.UtcNow;
        }

        public void MarkAsUsed()
        {
            UsedAt = DateTime.UtcNow;
        }

        public bool IsExpired => DateTime.UtcNow > ExpiresAt;
        public bool IsUsed => UsedAt.HasValue;
        public bool IsValid => !IsExpired && !IsUsed;
    }
} 