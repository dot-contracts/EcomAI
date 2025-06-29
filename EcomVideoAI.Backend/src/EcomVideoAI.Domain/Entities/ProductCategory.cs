namespace EcomVideoAI.Domain.Entities
{
    public class ProductCategory : AuditableEntity
    {
        public Guid Id { get; private set; }
        public required string Name { get; set; }
        public string? Description { get; set; }
        public Guid? ParentId { get; set; }
        public int Level { get; private set; } = 0;
        public int SortOrder { get; set; } = 0;
        public bool IsActive { get; set; } = true;

        // Navigation Properties
        public virtual ProductCategory? Parent { get; set; }
        public virtual ICollection<ProductCategory> Children { get; private set; } = new List<ProductCategory>();
        public virtual ICollection<ProductData> Products { get; private set; } = new List<ProductData>();

        private ProductCategory() { } // EF Core constructor

        public ProductCategory(string name, string? description = null, Guid? parentId = null)
        {
            Id = Guid.NewGuid();
            Name = name ?? throw new ArgumentNullException(nameof(name));
            Description = description;
            ParentId = parentId;
            CreatedAt = DateTime.UtcNow;
        }

        public void SetParent(Guid? parentId, int level)
        {
            ParentId = parentId;
            Level = level;
            UpdatedAt = DateTime.UtcNow;
        }

        public void Activate()
        {
            IsActive = true;
            UpdatedAt = DateTime.UtcNow;
        }

        public void Deactivate()
        {
            IsActive = false;
            UpdatedAt = DateTime.UtcNow;
        }

        public void UpdateSortOrder(int sortOrder)
        {
            SortOrder = sortOrder;
            UpdatedAt = DateTime.UtcNow;
        }

        public bool IsRoot => ParentId == null;
        public bool HasChildren => Children.Any();
        public string FullPath => Parent != null ? $"{Parent.FullPath} > {Name}" : Name;
    }
} 