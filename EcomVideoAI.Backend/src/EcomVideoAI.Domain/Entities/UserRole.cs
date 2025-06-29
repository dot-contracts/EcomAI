namespace EcomVideoAI.Domain.Entities
{
    public class UserRole
    {
        public Guid Id { get; private set; }
        public Guid UserId { get; private set; }
        public string RoleName { get; private set; } = string.Empty;
        public DateTime CreatedAt { get; private set; }
        public string? CreatedBy { get; set; }

        // Navigation Properties
        public virtual User User { get; set; } = null!;

        private UserRole() { } // EF Core constructor

        public UserRole(Guid userId, string roleName, string? createdBy = null)
        {
            Id = Guid.NewGuid();
            UserId = userId;
            RoleName = roleName ?? throw new ArgumentNullException(nameof(roleName));
            CreatedBy = createdBy;
            CreatedAt = DateTime.UtcNow;
        }
    }
} 