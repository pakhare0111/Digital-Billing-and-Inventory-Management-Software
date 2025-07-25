using System.ComponentModel.DataAnnotations;

namespace Billing_Software.Models
{
    public class Vehicle
    {
        public int Id { get; set; }

        [Required]
        public int CustomerId { get; set; }

        [Required]
        [StringLength(50)]
        public string Brand { get; set; } = string.Empty;

        [Required]
        [StringLength(50)]
        public string Model { get; set; } = string.Empty;

        [StringLength(10)]
        public string? Year { get; set; }

        [StringLength(20)]
        public string? PlateNumber { get; set; }

        [StringLength(50)]
        public string? Color { get; set; }

        [StringLength(50)]
        public string? EngineNumber { get; set; }

        [StringLength(50)]
        public string? ChassisNumber { get; set; }

        public DateTime CreatedDate { get; set; } = DateTime.Now;

        // Navigation property
        public virtual Customer Customer { get; set; } = null!;
        public virtual ICollection<ServiceRecord> ServiceRecords { get; set; } = new List<ServiceRecord>();
    }
} 