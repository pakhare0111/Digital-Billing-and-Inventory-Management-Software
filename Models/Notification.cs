using System.ComponentModel.DataAnnotations;

namespace Billing_Software.Models
{
    public enum NotificationType
    {
        Email,
        WhatsApp,
        SMS
    }

    public enum NotificationStatus
    {
        Pending,
        Sent,
        Failed,
        Delivered
    }

    public class Notification
    {
        public int Id { get; set; }

        [Required]
        public NotificationType Type { get; set; }

        [Required]
        [StringLength(100)]
        public string Recipient { get; set; } = string.Empty;

        [Required]
        [StringLength(200)]
        public string Subject { get; set; } = string.Empty;

        [Required]
        public string Message { get; set; } = string.Empty;

        public NotificationStatus Status { get; set; } = NotificationStatus.Pending;

        public DateTime CreatedDate { get; set; } = DateTime.Now;

        public DateTime? SentDate { get; set; }

        [StringLength(500)]
        public string? ErrorMessage { get; set; }

        public int? InvoiceId { get; set; }

        public int? ServiceRecordId { get; set; }

        public int? CustomerId { get; set; }

        // Navigation properties
        public virtual Invoice? Invoice { get; set; }
        public virtual ServiceRecord? ServiceRecord { get; set; }
        public virtual Customer? Customer { get; set; }
    }
} 