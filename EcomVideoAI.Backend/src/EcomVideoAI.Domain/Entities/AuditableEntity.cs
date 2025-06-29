namespace EcomVideoAI.Domain.Entities
{
    public abstract class AuditableEntity
    {
        public DateTime CreatedAt { get; protected set; }
        public DateTime? UpdatedAt { get; protected set; }
        public string? CreatedBy { get; protected set; }
        public string? UpdatedBy { get; protected set; }

        protected AuditableEntity()
        {
            CreatedAt = DateTime.UtcNow;
        }

        public void SetAuditInfo(string? createdBy = null, string? updatedBy = null)
        {
            if (CreatedBy == null && createdBy != null)
                CreatedBy = createdBy;
            
            if (updatedBy != null)
            {
                UpdatedBy = updatedBy;
                UpdatedAt = DateTime.UtcNow;
            }
        }
    }
} 