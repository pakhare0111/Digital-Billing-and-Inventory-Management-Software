using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;

namespace Billing_Software.Models
{
    public enum InvoiceStatus
    {
        Draft,
        Unpaid,
        Sent,
        Paid,
        Overdue,
        Cancelled
    }

    public class Invoice
    {
        public int Id { get; set; }

        // [Required]
        public int CustomerId { get; set; }

        [StringLength(20)]
        public string? InvoiceNumber { get; set; }

        [Required]
        public DateTime InvoiceDate { get; set; } = DateTime.Now;

        public DateTime? DueDate { get; set; }

        [Range(0, double.MaxValue)]
        public decimal Subtotal { get; set; }

        [Range(0, double.MaxValue)]
        public decimal TaxAmount { get; set; }

        [Range(0, double.MaxValue)]
        public decimal Total { get; set; }

        [Range(0, double.MaxValue)]
        public decimal AmountPaid { get; set; }

        [Range(0, double.MaxValue)]
        public decimal Balance { get; set; }

        public InvoiceStatus Status { get; set; } = InvoiceStatus.Draft;

        [StringLength(500)]
        public string? Notes { get; set; }

        [StringLength(100)]
        public string? PaymentMethod { get; set; }

        public DateTime? PaymentDate { get; set; }

        public DateTime CreatedDate { get; set; } = DateTime.Now;

        public DateTime? LastModified { get; set; }

        // Navigation properties
        [ValidateNever]
        public virtual Customer Customer { get; set; } = null!;
        public virtual ICollection<InvoiceItem> InvoiceItems { get; set; } = new List<InvoiceItem>();
    }
} 