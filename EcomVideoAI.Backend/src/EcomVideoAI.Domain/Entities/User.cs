using EcomVideoAI.Domain.ValueObjects;

namespace EcomVideoAI.Domain.Entities
{
    public class User : AuditableEntity
    {
        public Guid Id { get; private set; }
        public string Email { get; private set; } = string.Empty;
        public bool EmailVerified { get; private set; }
        public string PasswordHash { get; private set; } = string.Empty;
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public string? AvatarUrl { get; set; }
        public string? PhoneNumber { get; set; }
        public DateTime? DateOfBirth { get; set; }
        public string Timezone { get; set; } = "UTC";
        public string Locale { get; set; } = "en-US";
        public bool IsActive { get; private set; } = true;
        public DateTime? LastLoginAt { get; private set; }
        public int FailedLoginAttempts { get; private set; } = 0;
        public DateTime? LockedUntil { get; private set; }

        // Navigation Properties
        public virtual ICollection<Video> Videos { get; private set; } = new List<Video>();
        public virtual ICollection<UserSubscription> Subscriptions { get; private set; } = new List<UserSubscription>();
        public virtual ICollection<PaymentMethod> PaymentMethods { get; private set; } = new List<PaymentMethod>();
        public virtual ICollection<ProductData> ProductData { get; private set; } = new List<ProductData>();
        public virtual ICollection<RefreshToken> RefreshTokens { get; private set; } = new List<RefreshToken>();
        public virtual ICollection<UserRole> Roles { get; private set; } = new List<UserRole>();
        public virtual UserPreferences? Preferences { get; set; }
        public virtual NotificationSettings? NotificationSettings { get; set; }

        private User() { } // EF Core constructor

        public User(string email, string passwordHash, string? firstName = null, string? lastName = null)
        {
            Id = Guid.NewGuid();
            Email = email ?? throw new ArgumentNullException(nameof(email));
            PasswordHash = passwordHash ?? throw new ArgumentNullException(nameof(passwordHash));
            FirstName = firstName;
            LastName = lastName;
            EmailVerified = false;
            CreatedAt = DateTime.UtcNow;
        }

        public void VerifyEmail()
        {
            EmailVerified = true;
            UpdatedAt = DateTime.UtcNow;
        }

        public void UpdatePassword(string newPasswordHash)
        {
            PasswordHash = newPasswordHash ?? throw new ArgumentNullException(nameof(newPasswordHash));
            FailedLoginAttempts = 0;
            LockedUntil = null;
            UpdatedAt = DateTime.UtcNow;
        }

        public void RecordSuccessfulLogin()
        {
            LastLoginAt = DateTime.UtcNow;
            FailedLoginAttempts = 0;
            LockedUntil = null;
            UpdatedAt = DateTime.UtcNow;
        }

        public void RecordFailedLogin(int maxAttempts = 5, int lockoutMinutes = 30)
        {
            FailedLoginAttempts++;
            
            if (FailedLoginAttempts >= maxAttempts)
            {
                LockedUntil = DateTime.UtcNow.AddMinutes(lockoutMinutes);
            }
            
            UpdatedAt = DateTime.UtcNow;
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

        public bool IsLocked => LockedUntil.HasValue && LockedUntil > DateTime.UtcNow;

        public string FullName => $"{FirstName} {LastName}".Trim();
    }
} 