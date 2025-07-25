using Microsoft.AspNetCore.Mvc;
using Billing_Software.Models;
using Billing_Software.Services;
using Microsoft.EntityFrameworkCore;

namespace Billing_Software.Controllers
{
    public class CustomerController : Controller
    {
        private readonly ICustomerService _customerService;
        private readonly IInvoiceService _invoiceService;
        private readonly IServiceRecordService _serviceRecordService;
        private readonly ICustomerJobListService _customerJobListService;
        private readonly IJobListService _jobListService; // Added for GetAllAvailableJobs

        public CustomerController(
            ICustomerService customerService,
            IInvoiceService invoiceService,
            IServiceRecordService serviceRecordService,
            ICustomerJobListService customerJobListService,
            IJobListService jobListService) // Added jobListService to constructor
        {
            _customerService = customerService;
            _invoiceService = invoiceService;
            _serviceRecordService = serviceRecordService;
            _customerJobListService = customerJobListService;
            _jobListService = jobListService; // Initialize jobListService
        }

        // GET: Customer
        public async Task<IActionResult> Index(string searchTerm, string filter)
        {
            ViewBag.SearchTerm = searchTerm;
            ViewBag.Filter = filter;

            IEnumerable<Customer> customers;

            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                customers = await _customerService.SearchCustomersAsync(searchTerm);
            }
            else if (!string.IsNullOrWhiteSpace(filter))
            {
                customers = filter.ToLower() == "active" 
                    ? (await _customerService.GetAllCustomersAsync()).Where(c => c.IsActive)
                    : (await _customerService.GetAllCustomersAsync()).Where(c => !c.IsActive);
            }
            else
            {
                customers = await _customerService.GetAllCustomersAsync();
            }

            return View(customers);
        }

        // GET: Customer/Details/5
        public async Task<IActionResult> Details(int id)
        {
            var customer = await _customerService.GetCustomerByIdAsync(id);
            if (customer == null)
            {
                return NotFound();
            }

            // Get related data
            ViewBag.Invoices = await _customerService.GetCustomerInvoicesAsync(id);
            ViewBag.ServiceRecords = await _customerService.GetCustomerServiceRecordsAsync(id);
            ViewBag.Vehicles = await _customerService.GetCustomerVehiclesAsync(id);

            return View(customer);
        }

        // GET: Customer/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Customer/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Name,Phone,Email,Address,City,State,ZipCode")] Customer customer)
        {
            if (ModelState.IsValid)
            {
                // Check if customer with same phone already exists
                if (await _customerService.CustomerExistsAsync(customer.Phone))
                {
                    ModelState.AddModelError("Phone", "A customer with this phone number already exists.");
                    return View(customer);
                }

                await _customerService.CreateCustomerAsync(customer);
                TempData["SuccessMessage"] = "Customer created successfully!";
                return RedirectToAction(nameof(Index));
            }
            return View(customer);
        }

        // GET: Customer/Edit/5
        public async Task<IActionResult> Edit(int id)
        {
            var customer = await _customerService.GetCustomerByIdAsync(id);
            if (customer == null)
            {
                return NotFound();
            }
            return View(customer);
        }

        // POST: Customer/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Name,Phone,Email,Address,City,State,ZipCode,IsActive")] Customer customer)
        {
            if (id != customer.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    await _customerService.UpdateCustomerAsync(customer);
                    TempData["SuccessMessage"] = "Customer updated successfully!";
                    return RedirectToAction(nameof(Index));
                }
                catch (ArgumentException)
                {
                    return NotFound();
                }
            }
            return View(customer);
        }

        // GET: Customer/Delete/5
        public async Task<IActionResult> Delete(int id)
        {
            var customer = await _customerService.GetCustomerByIdAsync(id);
            if (customer == null)
            {
                return NotFound();
            }

            return View(customer);
        }

        // POST: Customer/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            try
            {
                var result = await _customerService.DeleteCustomerAsync(id);
                if (result)
                {
                    TempData["SuccessMessage"] = "Customer and all related data deleted successfully!";
                }
                else
                {
                    TempData["ErrorMessage"] = "Customer not found or could not be deleted.";
                }
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Error deleting customer: {ex.Message}";
            }
            return RedirectToAction(nameof(Index));
        }

        // GET: Customer/Vehicles/5
        public async Task<IActionResult> Vehicles(int id)
        {
            var customer = await _customerService.GetCustomerByIdAsync(id);
            if (customer == null)
            {
                return NotFound();
            }

            var vehicles = await _customerService.GetCustomerVehiclesAsync(id);
            ViewBag.Customer = customer;
            return View(vehicles);
        }

        // GET: Customer/Invoices/5
        public async Task<IActionResult> Invoices(int id)
        {
            var customer = await _customerService.GetCustomerByIdAsync(id);
            if (customer == null)
            {
                return NotFound();
            }

            var invoices = await _customerService.GetCustomerInvoicesAsync(id);
            ViewBag.Customer = customer;
            return View(invoices);
        }

        // GET: Customer/Services/5
        public async Task<IActionResult> Services(int id)
        {
            var customer = await _customerService.GetCustomerByIdAsync(id);
            if (customer == null)
            {
                return NotFound();
            }

            var serviceRecords = await _customerService.GetCustomerServiceRecordsAsync(id);
            ViewBag.Customer = customer;
            return View(serviceRecords);
        }

        // GET: Customer/Jobs/5
        public async Task<IActionResult> Jobs(int id)
        {
            var customer = await _customerService.GetCustomerByIdAsync(id);
            if (customer == null)
            {
                return NotFound();
            }

            // Initialize customer job lists if needed
            await _customerJobListService.InitializeCustomerJobListsAsync(id);

            var customerJobLists = await _customerJobListService.GetCustomerJobListsAsync(id);
            ViewBag.Customer = customer;
            return View(customerJobLists);
        }

        // POST: Customer/ToggleJobSelection/5
        [HttpPost]
        public async Task<IActionResult> ToggleJobSelection(int customerId, int jobListId)
        {
            try
            {
                var isSelected = await _customerJobListService.ToggleJobSelectionAsync(customerId, jobListId);
                return Json(new { success = true, isSelected = isSelected });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        // POST: Customer/MarkJobCompleted/5
        [HttpPost]
        public async Task<IActionResult> MarkJobCompleted(int customerId, int jobListId)
        {
            try
            {
                var result = await _customerJobListService.MarkJobAsCompletedAsync(customerId, jobListId);
                return Json(new { success = result });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        // POST: Customer/MarkJobIncomplete/5
        [HttpPost]
        public async Task<IActionResult> MarkJobIncomplete(int customerId, int jobListId)
        {
            try
            {
                var result = await _customerJobListService.MarkJobAsIncompleteAsync(customerId, jobListId);
                return Json(new { success = result });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        // GET: Customer/GetSelectedJobs/5
        [HttpGet]
        public async Task<IActionResult> GetSelectedJobs(int customerId)
        {
            var selectedJobs = await _customerJobListService.GetSelectedCustomerJobListsAsync(customerId);
            return Json(selectedJobs.Select(cjl => new
            {
                id = cjl.JobList.Id,
                jobName = cjl.JobList.JobName,
                description = cjl.JobList.Description,
                price = cjl.JobList.Price
            }));
        }

        // GET: Customer/GetCompletedJobs/5
        [HttpGet]
        public async Task<IActionResult> GetCompletedJobs(int customerId)
        {
            var completedJobs = await _customerJobListService.GetCompletedCustomerJobListsAsync(customerId);
            return Json(completedJobs.Select(cjl => new
            {
                id = cjl.JobList.Id,
                jobName = cjl.JobList.JobName,
                description = cjl.JobList.Description,
                price = cjl.JobList.Price
            }));
        }

        // POST: Customer/AddSelectedJobs/5
        [HttpPost]
        public async Task<IActionResult> AddSelectedJobs([FromBody] AddSelectedJobsRequest request)
        {
            try
            {
                if (request == null)
                {
                    return Json(new { success = false, message = "Invalid request data" });
                }

                var customer = await _customerService.GetCustomerByIdAsync(request.CustomerId);
                if (customer == null)
                {
                    return Json(new { success = false, message = "Customer not found" });
                }

                if (request.JobIds == null || request.JobIds.Length == 0)
                {
                    return Json(new { success = false, message = "No jobs selected" });
                }

                // Initialize customer job lists if needed
                await _customerJobListService.InitializeCustomerJobListsAsync(request.CustomerId);

                // Update selected jobs
                foreach (var jobId in request.JobIds)
                {
                    var customerJobList = await _customerJobListService.GetCustomerJobListByCustomerAndJobAsync(request.CustomerId, jobId);
                    if (customerJobList != null)
                    {
                        customerJobList.IsSelected = true;
                        customerJobList.LastModified = DateTime.Now;
                        await _customerJobListService.UpdateCustomerJobListAsync(customerJobList);
                    }
                }

                return Json(new { success = true, message = "Selected jobs have been added successfully" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Failed to add selected jobs: " + ex.Message });
            }
        }

        // Request model for AddSelectedJobs
        public class AddSelectedJobsRequest
        {
            public int CustomerId { get; set; }
            public int[] JobIds { get; set; } = new int[0];
        }

        [HttpPost]
        public async Task<IActionResult> ResetJobsForNewVisit(int customerId)
        {
            try
            {
                await _customerJobListService.ResetJobsForNewVisitAsync(customerId);
                return Json(new { success = true, message = "Jobs reset successfully for new visit!" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = $"Error resetting jobs: {ex.Message}" });
            }
        }

        [HttpGet]
        public async Task<IActionResult> JobHistory(int customerId)
        {
            var jobHistory = await _customerJobListService.GetJobHistoryAsync(customerId);
            return View(jobHistory);
        }

        // GET: Customer/GetCustomerJobs/5
        [HttpGet]
        public async Task<IActionResult> GetCustomerJobs(int customerId)
        {
            try
            {
                // Get all jobs from JobList table
                var allJobs = await _jobListService.GetAllJobListsAsync();
                
                // Get customer's selected jobs to mark them as selected
                var customerJobLists = await _customerJobListService.GetCustomerJobListsAsync(customerId);
                var selectedJobIds = customerJobLists.Where(cjl => cjl.IsSelected).Select(cjl => cjl.JobListId).ToList();
                var completedJobIds = customerJobLists.Where(cjl => cjl.IsCompleted).Select(cjl => cjl.JobListId).ToList();
                
                var jobs = allJobs.Select(job => new
                {
                    id = job.Id,
                    jobName = job.JobName,
                    description = job.Description,
                    price = job.Price,
                    isSelected = selectedJobIds.Contains(job.Id),
                    isCompleted = completedJobIds.Contains(job.Id),
                    status = completedJobIds.Contains(job.Id) ? "Completed" : 
                             (selectedJobIds.Contains(job.Id) ? "Selected" : "Available")
                });
                
                return Json(jobs);
            }
            catch (Exception ex)
            {
                return Json(new { error = ex.Message });
            }
        }

        // GET: Customer/GetAllAvailableJobs/5
        [HttpGet]
        public async Task<IActionResult> GetAllAvailableJobs(int customerId)
        {
            try
            {
                // Get all jobs from JobList table
                var allJobs = await _jobListService.GetAllJobListsAsync();
                
                var jobs = allJobs.Select(job => new
                {
                    id = job.Id,
                    jobName = job.JobName,
                    description = job.Description,
                    price = job.Price,
                    isSelected = false,
                    isCompleted = false,
                    status = "Available"
                });
                
                return Json(jobs);
            }
            catch (Exception ex)
            {
                return Json(new { error = ex.Message });
            }
        }

        // POST: Customer/CreateDefaultJobs/5
        [HttpPost]
        public async Task<IActionResult> CreateDefaultJobs(int customerId)
        {
            try
            {
                // Initialize customer job lists if needed
                await _customerJobListService.InitializeCustomerJobListsAsync(customerId);
                
                return Json(new { success = true, message = "Default jobs created successfully!" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }
    }
} 