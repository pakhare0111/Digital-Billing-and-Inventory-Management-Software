using System.ComponentModel.DataAnnotations;

namespace Billing_Software.Models
{
    public class Customer
    {
        public int Id { get; set; }

        [Required]
        [StringLength(100)]
        public string Name { get; set; } = string.Empty;

        [Required]
        [StringLength(20)]
        public string Phone { get; set; } = string.Empty;

        [EmailAddress]
        [StringLength(100)]
        public string? Email { get; set; }

        [StringLength(200)]
        public string? Address { get; set; }

        [StringLength(100)]
        public string? City { get; set; }

        [StringLength(20)]
        public string? State { get; set; }

        [StringLength(10)]
        public string? ZipCode { get; set; }

        public DateTime CreatedDate { get; set; } = DateTime.Now;

        public bool IsActive { get; set; } = true;

        // Calculated properties for reports
        public decimal TotalRevenue => Invoices?.Sum(i => i.Total) ?? 0;
        public int InvoiceCount => Invoices?.Count ?? 0;
        public decimal AverageInvoice => InvoiceCount > 0 ? TotalRevenue / InvoiceCount : 0;
        public DateTime? LastInvoiceDate => Invoices?.Any() == true ? Invoices.Max(i => i.InvoiceDate) : null;
        public int ServiceCount => ServiceRecords?.Count ?? 0;
        public decimal TotalSpent => ServiceRecords?.Where(s => s.Cost.HasValue).Sum(s => s.Cost ?? 0) ?? 0;
        public decimal AverageSpent => ServiceCount > 0 ? TotalSpent / ServiceCount : 0;
        public decimal PercentageOfTotal { get; set; }
        public int Rank { get; set; }

        // Navigation properties
        public virtual ICollection<Vehicle> Vehicles { get; set; } = new List<Vehicle>();
        public virtual ICollection<Invoice> Invoices { get; set; } = new List<Invoice>();
        public virtual ICollection<ServiceRecord> ServiceRecords { get; set; } = new List<ServiceRecord>();
        public virtual ICollection<CustomerJobList> CustomerJobLists { get; set; } = new List<CustomerJobList>();
    }
} 