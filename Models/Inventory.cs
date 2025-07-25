using System.ComponentModel.DataAnnotations;

namespace Billing_Software.Models
{
    public class Inventory
    {
        public int Id { get; set; }

        [Required]
        [StringLength(100)]
        public string Name { get; set; } = string.Empty;

        [StringLength(500)]
        public string? Description { get; set; }

        [Required]
        [StringLength(50)]
        public string Category { get; set; } = string.Empty;

        [Required]
        [Range(0, double.MaxValue)]
        public decimal Price { get; set; }

        [Required]
        [Range(0, int.MaxValue)]
        public int Quantity { get; set; }

        [Range(0, int.MaxValue)]
        public int MinStockLevel { get; set; } = 5;

        [StringLength(50)]
        public string? Unit { get; set; } = "pcs";

        [StringLength(100)]
        public string? Supplier { get; set; }

        [StringLength(50)]
        public string? SKU { get; set; }

        public DateTime CreatedDate { get; set; } = DateTime.Now;

        public DateTime? LastUpdated { get; set; }

        public bool IsActive { get; set; } = true;

        // Calculated properties for reports
        public decimal UnitPrice => Price;
        public decimal TotalValue => Price * Quantity;

        // Navigation property
        public virtual ICollection<InvoiceItem> InvoiceItems { get; set; } = new List<InvoiceItem>();
    }
} 