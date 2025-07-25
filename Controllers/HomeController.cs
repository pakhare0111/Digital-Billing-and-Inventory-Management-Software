using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Billing_Software.Services;
using Billing_Software.Models;

namespace Billing_Software.Controllers
{
    public class HomeController : Controller
    {
        private readonly IInvoiceService _invoiceService;
        private readonly ICustomerService _customerService;
        private readonly IInventoryService _inventoryService;
        private readonly IServiceRecordService _serviceRecordService;

        public HomeController(
            IInvoiceService invoiceService,
            ICustomerService customerService,
            IInventoryService inventoryService,
            IServiceRecordService serviceRecordService)
        {
            _invoiceService = invoiceService;
            _customerService = customerService;
            _inventoryService = inventoryService;
            _serviceRecordService = serviceRecordService;
        }

        public async Task<IActionResult> Index()
        {
            try
            {
                // Get dashboard statistics
                var invoices = await _invoiceService.GetAllInvoicesAsync();
                var customers = await _customerService.GetAllCustomersAsync();
                var inventory = await _inventoryService.GetAllInventoryAsync();
                var services = await _serviceRecordService.GetAllServiceRecordsAsync();

                var dashboardViewModel = new DashboardViewModel
                {
                    TotalRevenue = invoices.Sum(i => i.Total),
                    TotalInvoices = invoices.Count(),
                    TotalCustomers = customers.Count(),
                    LowStockItems = inventory.Count(i => i.Quantity <= i.MinStockLevel),
                    PendingInvoices = invoices.Count(i => i.Status == InvoiceStatus.Sent),
                    OverdueInvoices = invoices.Count(i => i.Status == InvoiceStatus.Overdue),
                    UpcomingServices = services.Count(s => s.Status == ServiceStatus.Scheduled && s.ServiceDate.Date >= DateTime.Today),
                    RecentInvoices = invoices.OrderByDescending(i => i.CreatedDate).Take(5).ToList(),
                    RecentServices = services.OrderByDescending(s => s.CreatedDate).Take(5).ToList()
                };

                return View(dashboardViewModel);
            }
            catch
            {
                return View("Error", new ErrorViewModel { RequestId = HttpContext.TraceIdentifier });
            }
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = HttpContext.TraceIdentifier });
        }
    }
}
