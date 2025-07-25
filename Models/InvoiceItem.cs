using System.ComponentModel.DataAnnotations;

namespace Billing_Software.Models
{
    public class InvoiceItem
    {
        public int Id { get; set; }

        public int InvoiceId { get; set; }

        [Required]
        [StringLength(100)]
        public string ItemName { get; set; } = string.Empty;

        [StringLength(500)]
        public string? Description { get; set; }

        [Required]
        [Range(0, int.MaxValue)]
        public int Quantity { get; set; }

        [Required]
        [Range(0, double.MaxValue)]
        public decimal UnitPrice { get; set; }

        [Required]
        [Range(0, double.MaxValue)]
        public decimal Total { get; set; }

        public int? InventoryId { get; set; }

        [StringLength(50)]
        public string? Unit { get; set; } = "pcs";

        // Navigation properties
        public virtual Invoice? Invoice { get; set; }
        public virtual Inventory? Inventory { get; set; }
    }
} 