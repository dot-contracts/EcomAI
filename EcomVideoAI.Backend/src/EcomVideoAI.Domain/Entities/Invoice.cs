using EcomVideoAI.Domain.Enums;

namespace EcomVideoAI.Domain.Entities
{
    public class Invoice : AuditableEntity
    {
        public Guid Id { get; private set; }
        public Guid PaymentId { get; private set; }
        public string InvoiceNumber { get; private set; } = string.Empty;
        public decimal Amount { get; private set; }
        public decimal TaxAmount { get; set; } = 0;
        public string Currency { get; set; } = "USD";
        public InvoiceStatus Status { get; private set; }
        public DateTime IssuedAt { get; private set; }
        public DateTime? DueAt { get; set; }
        public DateTime? PaidAt { get; private set; }
        public string? PdfUrl { get; set; }
        public Dictionary<string, object> Metadata { get; set; } = new Dictionary<string, object>();

        // Navigation Properties
        public virtual Payment Payment { get; set; } = null!;

        private Invoice() { } // EF Core constructor

        public Invoice(
            Guid paymentId,
            string invoiceNumber,
            decimal amount,
            decimal taxAmount = 0,
            DateTime? dueAt = null)
        {
            Id = Guid.NewGuid();
            PaymentId = paymentId;
            InvoiceNumber = invoiceNumber ?? throw new ArgumentNullException(nameof(invoiceNumber));
            Amount = amount;
            TaxAmount = taxAmount;
            DueAt = dueAt;
            Status = InvoiceStatus.Draft;
            IssuedAt = DateTime.UtcNow;
            CreatedAt = DateTime.UtcNow;
        }

        public void Send()
        {
            Status = InvoiceStatus.Sent;
            UpdatedAt = DateTime.UtcNow;
        }

        public void MarkAsPaid()
        {
            Status = InvoiceStatus.Paid;
            PaidAt = DateTime.UtcNow;
            UpdatedAt = DateTime.UtcNow;
        }

        public void Cancel()
        {
            Status = InvoiceStatus.Cancelled;
            UpdatedAt = DateTime.UtcNow;
        }

        public void SetPdfUrl(string pdfUrl)
        {
            PdfUrl = pdfUrl;
            UpdatedAt = DateTime.UtcNow;
        }

        public decimal TotalAmount => Amount + TaxAmount;
        public bool IsOverdue => DueAt.HasValue && DueAt < DateTime.UtcNow && Status != InvoiceStatus.Paid;
        public bool IsPaid => Status == InvoiceStatus.Paid;
        public int DaysOverdue => IsOverdue ? (DateTime.UtcNow - DueAt!.Value).Days : 0;
    }
} 