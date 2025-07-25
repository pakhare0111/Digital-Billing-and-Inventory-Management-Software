using System.ComponentModel.DataAnnotations;

namespace Billing_Software.Models
{
    public enum ServiceStatus
    {
        Scheduled,
        InProgress,
        Completed,
        Cancelled
    }

    public class ServiceRecord
    {
        public int Id { get; set; }

        [Required]
        public int CustomerId { get; set; }

        public int? VehicleId { get; set; }

        [Required]
        [StringLength(100)]
        public string ServiceType { get; set; } = string.Empty;

        [StringLength(500)]
        public string? Description { get; set; }

        [Required]
        public DateTime ServiceDate { get; set; }

        public DateTime? NextServiceDate { get; set; }

        [Range(0, double.MaxValue)]
        public decimal? Cost { get; set; }

        public ServiceStatus Status { get; set; } = ServiceStatus.Scheduled;

        [StringLength(1000)]
        public string? Notes { get; set; }

        [StringLength(100)]
        public string? Technician { get; set; }

        public int? InvoiceId { get; set; }

        public DateTime CreatedDate { get; set; } = DateTime.Now;

        public DateTime? LastModified { get; set; }

        // Navigation properties
        public virtual Customer Customer { get; set; } = null!;
        public virtual Vehicle? Vehicle { get; set; }
        public virtual Invoice? Invoice { get; set; }
    }
} 