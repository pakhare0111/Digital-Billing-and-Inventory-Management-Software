using Microsoft.AspNetCore.Mvc;
using Billing_Software.Models;
using Billing_Software.Services;
using Microsoft.EntityFrameworkCore;

namespace Billing_Software.Controllers
{
    public class ServiceController : Controller
    {
        private readonly IServiceRecordService _serviceRecordService;
        private readonly ICustomerService _customerService;
        private readonly IInvoiceService _invoiceService;

        public ServiceController(
            IServiceRecordService serviceRecordService,
            ICustomerService customerService,
            IInvoiceService invoiceService)
        {
            _serviceRecordService = serviceRecordService;
            _customerService = customerService;
            _invoiceService = invoiceService;
        }

        // GET: Service
        public async Task<IActionResult> Index(string searchTerm, string status, string customerFilter)
        {
            ViewBag.SearchTerm = searchTerm;
            ViewBag.Status = status;
            ViewBag.CustomerFilter = customerFilter;

            IEnumerable<ServiceRecord> services;

            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                // Search by service type, customer name, or vehicle details
                var allServices = await _serviceRecordService.GetAllServiceRecordsAsync();
                services = allServices.Where(s => 
                    s.ServiceType.Contains(searchTerm, StringComparison.OrdinalIgnoreCase) ||
                    s.Customer.Name.Contains(searchTerm, StringComparison.OrdinalIgnoreCase) ||
                    (s.Vehicle != null && (s.Vehicle.Brand.Contains(searchTerm, StringComparison.OrdinalIgnoreCase) ||
                                          s.Vehicle.Model.Contains(searchTerm, StringComparison.OrdinalIgnoreCase) ||
                                          s.Vehicle.PlateNumber?.Contains(searchTerm, StringComparison.OrdinalIgnoreCase) == true))
                );
            }
            else if (!string.IsNullOrWhiteSpace(status) && Enum.TryParse<ServiceStatus>(status, out var statusEnum))
            {
                services = await _serviceRecordService.GetServiceRecordsByStatusAsync(statusEnum);
            }
            else if (!string.IsNullOrWhiteSpace(customerFilter) && int.TryParse(customerFilter, out var customerId))
            {
                services = await _serviceRecordService.GetServiceRecordsByCustomerAsync(customerId);
            }
            else
            {
                services = await _serviceRecordService.GetAllServiceRecordsAsync();
            }

            return View(services);
        }

        // GET: Service/Details/5
        public async Task<IActionResult> Details(int id)
        {
            var serviceRecord = await _serviceRecordService.GetServiceRecordByIdAsync(id);
            if (serviceRecord == null)
            {
                return NotFound();
            }

            return View(serviceRecord);
        }

        // GET: Service/Create
        public async Task<IActionResult> Create(int? customerId)
        {
            ViewBag.Customers = await _customerService.GetAllCustomersAsync();
            ViewBag.ServiceStatuses = Enum.GetValues<ServiceStatus>();

            var serviceRecord = new ServiceRecord
            {
                CustomerId = customerId ?? 0,
                ServiceDate = DateTime.Now,
                Status = ServiceStatus.Scheduled
            };

            return View(serviceRecord);
        }

        // POST: Service/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("CustomerId,VehicleId,ServiceType,Description,ServiceDate,NextServiceDate,Cost,Status,Notes,Technician")] ServiceRecord serviceRecord)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    await _serviceRecordService.CreateServiceRecordAsync(serviceRecord);
                    TempData["SuccessMessage"] = "Service record created successfully!";
                    return RedirectToAction(nameof(Index));
                }
                catch (Exception ex)
                {
                    TempData["ErrorMessage"] = $"Error creating service record: {ex.Message}";
                }
            }

            ViewBag.Customers = await _customerService.GetAllCustomersAsync();
            ViewBag.ServiceStatuses = Enum.GetValues<ServiceStatus>();
            return View(serviceRecord);
        }

        // GET: Service/Edit/5
        public async Task<IActionResult> Edit(int id)
        {
            var serviceRecord = await _serviceRecordService.GetServiceRecordByIdAsync(id);
            if (serviceRecord == null)
            {
                return NotFound();
            }

            ViewBag.Customers = await _customerService.GetAllCustomersAsync();
            ViewBag.ServiceStatuses = Enum.GetValues<ServiceStatus>();
            return View(serviceRecord);
        }

        // POST: Service/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,CustomerId,VehicleId,ServiceType,Description,ServiceDate,NextServiceDate,Cost,Status,Notes,Technician")] ServiceRecord serviceRecord)
        {
            if (id != serviceRecord.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    await _serviceRecordService.UpdateServiceRecordAsync(serviceRecord);
                    TempData["SuccessMessage"] = "Service record updated successfully!";
                    return RedirectToAction(nameof(Index));
                }
                catch (ArgumentException)
                {
                    return NotFound();
                }
                catch (Exception ex)
                {
                    TempData["ErrorMessage"] = $"Error updating service record: {ex.Message}";
                }
            }

            ViewBag.Customers = await _customerService.GetAllCustomersAsync();
            ViewBag.ServiceStatuses = Enum.GetValues<ServiceStatus>();
            return View(serviceRecord);
        }

        // GET: Service/Delete/5
        public async Task<IActionResult> Delete(int id)
        {
            var serviceRecord = await _serviceRecordService.GetServiceRecordByIdAsync(id);
            if (serviceRecord == null)
            {
                return NotFound();
            }

            return View(serviceRecord);
        }

        // POST: Service/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            try
            {
                var result = await _serviceRecordService.DeleteServiceRecordAsync(id);
                if (result)
                {
                    TempData["SuccessMessage"] = "Service record deleted successfully!";
                }
                else
                {
                    TempData["ErrorMessage"] = "Service record not found or could not be deleted.";
                }
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Error deleting service record: {ex.Message}";
            }
            return RedirectToAction(nameof(Index));
        }

        // GET: Service/Upcoming
        public async Task<IActionResult> Upcoming()
        {
            var fromDate = DateTime.Today;
            var toDate = DateTime.Today.AddDays(30);
            var upcomingServices = await _serviceRecordService.GetUpcomingServicesAsync(fromDate, toDate);
            return View(upcomingServices);
        }

        // GET: Service/Overdue
        public async Task<IActionResult> Overdue()
        {
            var overdueServices = await _serviceRecordService.GetOverdueServicesAsync();
            return View(overdueServices);
        }

        // POST: Service/SendReminder/5
        [HttpPost]
        public async Task<IActionResult> SendReminder(int id)
        {
            try
            {
                var result = await _serviceRecordService.SendServiceReminderAsync(id);
                if (result)
                {
                    TempData["SuccessMessage"] = "Service reminder sent successfully!";
                }
                else
                {
                    TempData["ErrorMessage"] = "Failed to send service reminder.";
                }
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Error sending reminder: {ex.Message}";
            }
            return RedirectToAction(nameof(Details), new { id });
        }

        // GET: Service/CustomerServices/5
        public async Task<IActionResult> CustomerServices(int customerId)
        {
            var customer = await _customerService.GetCustomerByIdAsync(customerId);
            if (customer == null)
            {
                return NotFound();
            }

            var services = await _serviceRecordService.GetServiceRecordsByCustomerAsync(customerId);
            ViewBag.Customer = customer;
            return View(services);
        }

        // GET: Service/VehicleServices/5
        public async Task<IActionResult> VehicleServices(int vehicleId)
        {
            var services = await _serviceRecordService.GetServiceRecordsByVehicleAsync(vehicleId);
            return View(services);
        }

        // POST: Service/UpdateStatus/5
        [HttpPost]
        public async Task<IActionResult> UpdateStatus(int id, ServiceStatus status)
        {
            try
            {
                var serviceRecord = await _serviceRecordService.GetServiceRecordByIdAsync(id);
                if (serviceRecord == null)
                {
                    return NotFound();
                }

                serviceRecord.Status = status;
                await _serviceRecordService.UpdateServiceRecordAsync(serviceRecord);
                TempData["SuccessMessage"] = "Service status updated successfully!";
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Error updating service status: {ex.Message}";
            }
            return RedirectToAction(nameof(Details), new { id });
        }
    }
} 