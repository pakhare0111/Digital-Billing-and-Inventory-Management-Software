using Microsoft.AspNetCore.Mvc;
using Billing_Software.Models;
using Billing_Software.Services;
using Microsoft.EntityFrameworkCore;

namespace Billing_Software.Controllers
{
    public class ReportController : Controller
    {
        private readonly IInvoiceService _invoiceService;
        private readonly ICustomerService _customerService;
        private readonly IInventoryService _inventoryService;
        private readonly IServiceRecordService _serviceRecordService;
        private readonly IPdfService _pdfService;

        public ReportController(
            IInvoiceService invoiceService,
            ICustomerService customerService,
            IInventoryService inventoryService,
            IServiceRecordService serviceRecordService,
            IPdfService pdfService)
        {
            _invoiceService = invoiceService;
            _customerService = customerService;
            _inventoryService = inventoryService;
            _serviceRecordService = serviceRecordService;
            _pdfService = pdfService;
        }

        // GET: Report
        public async Task<IActionResult> Index()
        {
            try
            {
                var invoices = await _invoiceService.GetAllInvoicesAsync();
                var customers = await _customerService.GetAllCustomersAsync();
                var inventory = await _inventoryService.GetAllInventoryAsync();
                var services = await _serviceRecordService.GetAllServiceRecordsAsync();

                var reportViewModel = new ReportViewModel
                {
                    TotalRevenue = invoices.Sum(i => i.Total),
                    TotalInvoices = invoices.Count(),
                    TotalCustomers = customers.Count(),
                    TotalServices = services.Count(),
                    AverageInvoiceValue = invoices.Any() ? invoices.Average(i => i.Total) : 0,
                    TopCustomers = customers.OrderByDescending(c => c.Invoices.Sum(i => i.Total)).Take(5).ToList(),
                    RecentInvoices = invoices.OrderByDescending(i => i.CreatedDate).Take(10).ToList(),
                    LowStockItems = inventory.Where(i => i.Quantity <= i.MinStockLevel).Count(),
                    InventoryValue = await _inventoryService.GetInventoryValueAsync()
                };

                return View(reportViewModel);
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Error generating report: {ex.Message}";
                return View(new ReportViewModel());
            }
        }

        // GET: Report/Revenue
        public async Task<IActionResult> Revenue(DateTime? startDate, DateTime? endDate)
        {
            var start = startDate ?? DateTime.Today.AddMonths(-1);
            var end = endDate ?? DateTime.Today;

            ViewBag.StartDate = start;
            ViewBag.EndDate = end;

            try
            {
                var invoices = await _invoiceService.GetAllInvoicesAsync();
                var filteredInvoices = invoices.Where(i => i.InvoiceDate >= start && i.InvoiceDate <= end);

                var revenueReport = new RevenueReportViewModel
                {
                    StartDate = start,
                    EndDate = end,
                    TotalRevenue = filteredInvoices.Sum(i => i.Total),
                    TotalInvoices = filteredInvoices.Count(),
                    PaidInvoices = filteredInvoices.Count(i => i.Status == InvoiceStatus.Paid),
                    PendingInvoices = filteredInvoices.Count(i => i.Status == InvoiceStatus.Sent),
                    OverdueInvoices = filteredInvoices.Count(i => i.Status == InvoiceStatus.Overdue),
                    DailyRevenue = filteredInvoices.GroupBy(i => i.InvoiceDate.Date)
                        .Select(g => new DailyRevenue { Date = g.Key, Amount = g.Sum(i => i.Total) })
                        .OrderBy(d => d.Date)
                        .ToList(),
                    MonthlyRevenue = filteredInvoices.GroupBy(i => new { i.InvoiceDate.Year, i.InvoiceDate.Month })
                        .Select(g => new MonthlyRevenue { Year = g.Key.Year, Month = g.Key.Month, Amount = g.Sum(i => i.Total) })
                        .OrderBy(m => m.Year).ThenBy(m => m.Month)
                        .ToList()
                };

                return View(revenueReport);
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Error generating revenue report: {ex.Message}";
                return View(new RevenueReportViewModel());
            }
        }

        // GET: Report/Customer
        public async Task<IActionResult> Customer(int? customerId)
        {
            if (customerId.HasValue)
            {
                var customer = await _customerService.GetCustomerByIdAsync(customerId.Value);
                if (customer == null)
                {
                    return NotFound();
                }

                var customerInvoices = await _invoiceService.GetInvoicesByCustomerAsync(customerId.Value);
                var customerServices = await _serviceRecordService.GetServiceRecordsByCustomerAsync(customerId.Value);

                var customerReport = new CustomerReportViewModel
                {
                    Customer = customer,
                    TotalSpent = customerInvoices.Sum(i => i.Total),
                    TotalInvoices = customerInvoices.Count(),
                    TotalServices = customerServices.Count(),
                    AverageInvoiceValue = customerInvoices.Any() ? customerInvoices.Average(i => i.Total) : 0,
                    LastServiceDate = customerServices.Any() ? customerServices.Max(s => s.ServiceDate) : null,
                    Invoices = customerInvoices.OrderByDescending(i => i.CreatedDate).ToList(),
                    Services = customerServices.OrderByDescending(s => s.ServiceDate).ToList()
                };

                return View(customerReport);
            }

            // All customers report
            var customers = await _customerService.GetAllCustomersAsync();
            var allCustomersReport = new AllCustomersReportViewModel
            {
                TotalCustomers = customers.Count(),
                ActiveCustomers = customers.Count(c => c.IsActive),
                TopSpenders = customers.OrderByDescending(c => c.Invoices.Sum(i => i.Total)).Take(10).ToList(),
                RecentCustomers = customers.OrderByDescending(c => c.CreatedDate).Take(10).ToList()
            };

            return View(allCustomersReport);
        }

        // GET: Report/Inventory
        public async Task<IActionResult> Inventory()
        {
            try
            {
                var inventory = await _inventoryService.GetAllInventoryAsync();
                var lowStockItems = await _inventoryService.GetLowStockItemsAsync();
                var outOfStockItems = await _inventoryService.GetOutOfStockItemsAsync();
                var totalValue = await _inventoryService.GetInventoryValueAsync();

                var inventoryReport = new InventoryReportViewModel
                {
                    TotalItems = inventory.Count(),
                    TotalValue = totalValue,
                    LowStockItems = lowStockItems.Count(),
                    OutOfStockItems = outOfStockItems.Count(),
                    AverageItemValue = inventory.Any() ? totalValue / inventory.Count() : 0,
                    LowStockItemsList = lowStockItems.ToList(),
                    OutOfStockItemsList = outOfStockItems.ToList(),
                    CategoryBreakdown = inventory.GroupBy(i => i.Category)
                        .Select(g => new CategoryBreakdown 
                        { 
                            Category = g.Key, 
                            Count = g.Count(),
                            ItemCount = g.Count(),
                            Value = g.Sum(i => i.Price * i.Quantity),
                            TotalValue = g.Sum(i => i.Price * i.Quantity),
                            AveragePrice = g.Average(i => i.Price),
                            LowStockCount = g.Count(i => i.Quantity <= i.MinStockLevel),
                            OutOfStockCount = g.Count(i => i.Quantity == 0)
                        })
                        .OrderByDescending(c => c.Value)
                        .ToList(),
                    TopValueItems = inventory.OrderByDescending(i => i.Price * i.Quantity).Take(10).ToList()
                };

                return View(inventoryReport);
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Error generating inventory report: {ex.Message}";
                return View(new InventoryReportViewModel());
            }
        }

        // GET: Report/Service
        public async Task<IActionResult> Service(DateTime? startDate, DateTime? endDate)
        {
            var start = startDate ?? DateTime.Today.AddMonths(-1);
            var end = endDate ?? DateTime.Today;

            ViewBag.StartDate = start;
            ViewBag.EndDate = end;

            try
            {
                var services = await _serviceRecordService.GetAllServiceRecordsAsync();
                var filteredServices = services.Where(s => s.ServiceDate >= start && s.ServiceDate <= end);

                var serviceReport = new ServiceReportViewModel
                {
                    StartDate = start,
                    EndDate = end,
                    TotalServices = filteredServices.Count(),
                    CompletedServices = filteredServices.Count(s => s.Status == ServiceStatus.Completed),
                    ScheduledServices = filteredServices.Count(s => s.Status == ServiceStatus.Scheduled),
                    TotalRevenue = filteredServices.Where(s => s.Cost.HasValue).Sum(s => s.Cost ?? 0),
                    AverageServiceCost = filteredServices.Where(s => s.Cost.HasValue).Any() 
                        ? filteredServices.Where(s => s.Cost.HasValue).Average(s => s.Cost ?? 0) : 0,
                    ServiceTypeBreakdown = filteredServices.GroupBy(s => s.ServiceType)
                        .Select(g => new ServiceTypeBreakdown 
                        { 
                            ServiceType = g.Key, 
                            Count = g.Count(), 
                            Revenue = g.Where(s => s.Cost.HasValue).Sum(s => s.Cost ?? 0) 
                        })
                        .OrderByDescending(s => s.Count)
                        .ToList(),
                    RecentServices = filteredServices.OrderByDescending(s => s.ServiceDate).Take(10).ToList()
                };

                return View(serviceReport);
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Error generating service report: {ex.Message}";
                return View(new ServiceReportViewModel());
            }
        }

        // GET: Report/Export/Invoice
        public async Task<IActionResult> ExportInvoice(int id)
        {
            try
            {
                var pdfBytes = await _pdfService.GenerateInvoicePdfAsync(id);
                if (pdfBytes.Length > 0)
                {
                    return File(pdfBytes, "application/pdf", $"Invoice_{id}.pdf");
                }
                else
                {
                    TempData["ErrorMessage"] = "Failed to generate PDF.";
                    return RedirectToAction("Details", "Invoice", new { id });
                }
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Error exporting invoice: {ex.Message}";
                return RedirectToAction("Details", "Invoice", new { id });
            }
        }

        // GET: Report/Export/Customer
        public async Task<IActionResult> ExportCustomer(int id)
        {
            try
            {
                var pdfBytes = await _pdfService.GenerateCustomerReportPdfAsync(id);
                if (pdfBytes.Length > 0)
                {
                    return File(pdfBytes, "application/pdf", $"Customer_{id}.pdf");
                }
                else
                {
                    TempData["ErrorMessage"] = "Failed to generate PDF.";
                    return RedirectToAction("Customer", new { customerId = id });
                }
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Error exporting customer report: {ex.Message}";
                return RedirectToAction("Customer", new { customerId = id });
            }
        }

        // GET: Report/Export/Inventory
        public async Task<IActionResult> ExportInventory()
        {
            try
            {
                var pdfBytes = await _pdfService.GenerateInventoryReportPdfAsync();
                if (pdfBytes.Length > 0)
                {
                    return File(pdfBytes, "application/pdf", $"Inventory_Report_{DateTime.Now:yyyyMMdd}.pdf");
                }
                else
                {
                    TempData["ErrorMessage"] = "Failed to generate PDF.";
                    return RedirectToAction("Inventory");
                }
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Error exporting inventory report: {ex.Message}";
                return RedirectToAction("Inventory");
            }
        }

        // GET: Report/Export/Service
        public async Task<IActionResult> ExportService(int id)
        {
            try
            {
                var pdfBytes = await _pdfService.GenerateServiceReportPdfAsync(id);
                if (pdfBytes.Length > 0)
                {
                    return File(pdfBytes, "application/pdf", $"Service_{id}.pdf");
                }
                else
                {
                    TempData["ErrorMessage"] = "Failed to generate PDF.";
                    return RedirectToAction("Details", "Service", new { id });
                }
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Error exporting service report: {ex.Message}";
                return RedirectToAction("Details", "Service", new { id });
            }
        }
    }
} 