using Billing_Software.Models;

namespace Billing_Software.Models
{
    public class ReportViewModel
    {
        public decimal TotalRevenue { get; set; }
        public int TotalInvoices { get; set; }
        public int TotalCustomers { get; set; }
        public int TotalServices { get; set; }
        public decimal AverageInvoiceValue { get; set; }
        public List<Customer> TopCustomers { get; set; } = new List<Customer>();
        public List<Invoice> RecentInvoices { get; set; } = new List<Invoice>();
        public int LowStockItems { get; set; }
        public decimal InventoryValue { get; set; }
    }

    public class RevenueReportViewModel
    {
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public decimal TotalRevenue { get; set; }
        public int TotalInvoices { get; set; }
        public int PaidInvoices { get; set; }
        public int PendingInvoices { get; set; }
        public int OverdueInvoices { get; set; }
        public decimal RevenueGrowth { get; set; }
        public int InvoiceGrowth { get; set; }
        public decimal AverageGrowth { get; set; }
        public decimal AverageInvoiceValue { get; set; }
        public string RevenueTrend { get; set; } = string.Empty;
        public int PeriodDays { get; set; }
        public List<DailyRevenue> DailyRevenue { get; set; } = new List<DailyRevenue>();
        public List<MonthlyRevenue> MonthlyRevenue { get; set; } = new List<MonthlyRevenue>();
        public List<Customer> TopCustomers { get; set; } = new List<Customer>();
        public List<RevenueByStatus> RevenueByStatus { get; set; } = new List<RevenueByStatus>();
    }

    public class RevenueByStatus
    {
        public InvoiceStatus Status { get; set; }
        public decimal Revenue { get; set; }
        public int Count { get; set; }
        public decimal Percentage { get; set; }
    }

    public class DailyRevenue
    {
        public DateTime Date { get; set; }
        public decimal Amount { get; set; }
        public decimal Revenue { get; set; }
    }

    public class MonthlyRevenue
    {
        public int Year { get; set; }
        public int Month { get; set; }
        public decimal Amount { get; set; }
        public decimal Revenue { get; set; }
        public int InvoiceCount { get; set; }
        public decimal AverageInvoice { get; set; }
        public decimal Growth { get; set; }
        public string MonthName => System.Globalization.CultureInfo.CurrentCulture.DateTimeFormat.GetMonthName(Month);
    }

    public class CustomerReportViewModel
    {
        public Customer Customer { get; set; } = null!;
        public decimal TotalSpent { get; set; }
        public int TotalInvoices { get; set; }
        public int TotalServices { get; set; }
        public decimal AverageInvoiceValue { get; set; }
        public DateTime? LastServiceDate { get; set; }
        public List<Invoice> Invoices { get; set; } = new List<Invoice>();
        public List<ServiceRecord> Services { get; set; } = new List<ServiceRecord>();
    }

    public class AllCustomersReportViewModel
    {
        public int TotalCustomers { get; set; }
        public int ActiveCustomers { get; set; }
        public decimal TotalRevenue { get; set; }
        public decimal AverageCustomerValue { get; set; }
        public int TotalVehicles { get; set; }
        public List<Customer> TopCustomers { get; set; } = new List<Customer>();
        public List<Customer> TopSpenders { get; set; } = new List<Customer>();
        public List<Customer> RecentCustomers { get; set; } = new List<Customer>();
        public List<CustomerGrowth> CustomerGrowth { get; set; } = new List<CustomerGrowth>();
        public List<CustomerSegment> CustomerSegments { get; set; } = new List<CustomerSegment>();
        public Customer? MostValuableCustomer { get; set; }
        public Customer? MostRecentCustomer { get; set; }
        public Customer? MostActiveCustomer { get; set; }
        public Customer? HighestAverageCustomer { get; set; }
    }

    public class CustomerGrowth
    {
        public string Period { get; set; } = string.Empty;
        public int NewCustomers { get; set; }
        public decimal GrowthRate { get; set; }
        public decimal TotalRevenue { get; set; }
    }

    public class CustomerSegment
    {
        public string Segment { get; set; } = string.Empty;
        public int Count { get; set; }
        public decimal Revenue { get; set; }
        public decimal AverageValue { get; set; }
    }

    public class InventoryReportViewModel
    {
        public int TotalItems { get; set; }
        public decimal TotalValue { get; set; }
        public int LowStockItems { get; set; }
        public int OutOfStockItems { get; set; }
        public decimal AverageItemValue { get; set; }
        public List<Inventory> LowStockItemsList { get; set; } = new List<Inventory>();
        public List<Inventory> OutOfStockItemsList { get; set; } = new List<Inventory>();
        public List<CategoryBreakdown> CategoryBreakdown { get; set; } = new List<CategoryBreakdown>();
        public List<Inventory> TopValueItems { get; set; } = new List<Inventory>();
        public Inventory? MostValuableItem { get; set; }
        public Inventory? MostStockedItem { get; set; }
        public CategoryBreakdown? TopCategory { get; set; }
    }

    public class CategoryBreakdown
    {
        public string Category { get; set; } = string.Empty;
        public int Count { get; set; }
        public int ItemCount { get; set; }
        public decimal Value { get; set; }
        public decimal TotalValue { get; set; }
        public decimal AveragePrice { get; set; }
        public int LowStockCount { get; set; }
        public int OutOfStockCount { get; set; }
    }

    public class ServiceReportViewModel
    {
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public int TotalServices { get; set; }
        public int CompletedServices { get; set; }
        public int ScheduledServices { get; set; }
        public int InProgressServices { get; set; }
        public int CancelledServices { get; set; }
        public decimal TotalRevenue { get; set; }
        public decimal AverageServiceCost { get; set; }
        public decimal CompletionRate { get; set; }
        public decimal ScheduledRate { get; set; }
        public decimal InProgressRate { get; set; }
        public decimal CancelledRate { get; set; }
        public List<ServiceTypeBreakdown> ServiceTypeBreakdown { get; set; } = new List<ServiceTypeBreakdown>();
        public List<ServiceRecord> RecentServices { get; set; } = new List<ServiceRecord>();
        public List<Customer> TopServiceCustomers { get; set; } = new List<Customer>();
        public List<ServiceTrend> ServiceTrends { get; set; } = new List<ServiceTrend>();
        public ServiceTypeBreakdown? MostPopularService { get; set; }
        public ServiceTypeBreakdown? HighestRevenueService { get; set; }
        public ServiceTypeBreakdown? HighestAverageService { get; set; }
        public Customer? TopServiceCustomer { get; set; }
    }

    public class ServiceTrend
    {
        public string Period { get; set; } = string.Empty;
        public int ServiceCount { get; set; }
        public decimal Revenue { get; set; }
        public decimal Growth { get; set; }
    }

    public class ServiceTypeBreakdown
    {
        public string ServiceType { get; set; } = string.Empty;
        public int Count { get; set; }
        public decimal Revenue { get; set; }
        public decimal AverageCost { get; set; }
        public decimal CompletionRate { get; set; }
    }
} 