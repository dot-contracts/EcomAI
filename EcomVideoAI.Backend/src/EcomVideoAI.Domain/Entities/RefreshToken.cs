namespace EcomVideoAI.Domain.Entities
{
    public class RefreshToken
    {
        public Guid Id { get; private set; }
        public Guid UserId { get; private set; }
        public string Token { get; private set; } = string.Empty;
        public DateTime ExpiresAt { get; private set; }
        public bool IsRevoked { get; private set; } = false;
        public DateTime CreatedAt { get; private set; }
        public DateTime? RevokedAt { get; private set; }

        // Navigation Properties
        public virtual User User { get; set; } = null!;

        private RefreshToken() { } // EF Core constructor

        public RefreshToken(Guid userId, string token, DateTime expiresAt)
        {
            Id = Guid.NewGuid();
            UserId = userId;
            Token = token ?? throw new ArgumentNullException(nameof(token));
            ExpiresAt = expiresAt;
            CreatedAt = DateTime.UtcNow;
        }

        public void Revoke()
        {
            IsRevoked = true;
            RevokedAt = DateTime.UtcNow;
        }

        public bool IsExpired => DateTime.UtcNow > ExpiresAt;
        public bool IsActive => !IsRevoked && !IsExpired;
    }
} 