using System.ComponentModel.DataAnnotations;

namespace Billing_Software.Models
{
    public class JobList
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Job Name is required")]
        public string JobName { get; set; } = string.Empty;

        public string? Description { get; set; }

        [Required(ErrorMessage = "Price is required")]
        [Range(0.01, double.MaxValue, ErrorMessage = "Price must be greater than 0")]
        public decimal Price { get; set; }

        public DateTime CreatedDate { get; set; } = DateTime.Now;

        public DateTime? LastModified { get; set; }
    }
} 