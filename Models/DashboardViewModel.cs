using Billing_Software.Models;

namespace Billing_Software.Models
{
    public class DashboardViewModel
    {
        public decimal TotalRevenue { get; set; }
        public int TotalInvoices { get; set; }
        public int TotalCustomers { get; set; }
        public int LowStockItems { get; set; }
        public int PendingInvoices { get; set; }
        public int OverdueInvoices { get; set; }
        public int UpcomingServices { get; set; }
        public List<Invoice> RecentInvoices { get; set; } = new List<Invoice>();
        public List<ServiceRecord> RecentServices { get; set; } = new List<ServiceRecord>();
    }
} 