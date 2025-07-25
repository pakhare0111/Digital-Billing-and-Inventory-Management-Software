using System.ComponentModel.DataAnnotations;

namespace Billing_Software.Models
{
    public class CustomerJobList
    {
        public int Id { get; set; }
        [Required]
        public int CustomerId { get; set; }
        [Required]
        public int JobListId { get; set; }
        public bool IsSelected { get; set; } = false;
        public bool IsCompleted { get; set; } = false;
        public DateTime? CompletedDate { get; set; }
        public DateTime CreatedDate { get; set; } = DateTime.Now;
        public DateTime? LastModified { get; set; }
        
        // New fields for invoice tracking
        public bool IsInvoiced { get; set; } = false;
        public int? InvoiceId { get; set; }
        public DateTime? InvoicedDate { get; set; }
        public bool IsResetForNewVisit { get; set; } = false;
        public DateTime? ResetDate { get; set; }
        
        public virtual Customer Customer { get; set; } = null!;
        public virtual JobList JobList { get; set; } = null!;
        public virtual Invoice? Invoice { get; set; }
    }
} 