using Microsoft.AspNetCore.Mvc;
using Billing_Software.Models;
using Billing_Software.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace Billing_Software.Controllers
{
    public class InvoiceController : Controller
    {
        private readonly IInvoiceService _invoiceService;
        private readonly ICustomerService _customerService;
        private readonly IInventoryService _inventoryService;
        private readonly ICustomerJobListService _customerJobListService;
        private readonly IJobListService _jobListService; // Added for GetAllAvailableJobs
        private readonly IWhatsAppService _whatsAppService; // Added WhatsApp service
        private readonly IPdfService _pdfService; // Added PDF service

        public InvoiceController(
            IInvoiceService invoiceService,
            ICustomerService customerService,
            IInventoryService inventoryService,
            ICustomerJobListService customerJobListService,
            IJobListService jobListService, // Added jobListService to constructor
            IWhatsAppService whatsAppService, // Added WhatsApp service to constructor
            IPdfService pdfService) // Added PDF service to constructor
        {
            _invoiceService = invoiceService;
            _customerService = customerService;
            _inventoryService = inventoryService;
            _customerJobListService = customerJobListService;
            _jobListService = jobListService; // Initialize jobListService
            _whatsAppService = whatsAppService; // Initialize WhatsApp service
            _pdfService = pdfService; // Initialize PDF service
        }

        // GET: Invoice
        public async Task<IActionResult> Index(string searchTerm, string status)
        {
            ViewBag.SearchTerm = searchTerm;
            ViewBag.Status = status;
            var invoices = await _invoiceService.SearchInvoicesAsync(searchTerm, status);
            return View(invoices);
        }

        // GET: Invoice/Details/5
        public async Task<IActionResult> Details(int id)
        {
            var invoice = await _invoiceService.GetInvoiceByIdAsync(id);
            if (invoice == null)
                return NotFound();
            return View(invoice);
        }

        // GET: Invoice/Create
        public async Task<IActionResult> Create(int? customerId)
        {
            ViewBag.Customers = await _customerService.GetAllCustomersAsync();
            ViewBag.Inventory = await _inventoryService.GetAllActiveInventoryAsync();
            
            var invoice = new Invoice
            {
                CustomerId = customerId ?? 0,
                InvoiceDate = DateTime.Now,
                DueDate = DateTime.Now.AddDays(7),
                InvoiceItems = new List<InvoiceItem>()
            };

            // If customer is selected, get their selected and completed jobs
            if (customerId.HasValue && customerId.Value > 0)
            {
                var selectedJobs = await _customerJobListService.GetSelectedCustomerJobListsAsync(customerId.Value);
                var completedJobs = await _customerJobListService.GetCompletedCustomerJobListsAsync(customerId.Value);
                ViewBag.SelectedJobs = selectedJobs;
                ViewBag.CompletedJobs = completedJobs;
            }

            return View(invoice);
        }

        // POST: Invoice/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Invoice invoice)
        {
            Console.WriteLine("[InvoiceController] Create POST action called");
            try
            {
                // Log received invoice data
                TempData["DebugMessage"] = $"[START] Received Invoice - CustomerId: {invoice.CustomerId}, InvoiceDate: {invoice.InvoiceDate}, DueDate: {invoice.DueDate}";
                
                // Set a temporary invoice number
                invoice.InvoiceNumber = "TEMP-" + DateTime.Now.Ticks;
                
                // Process invoice items from form
                var invoiceItems = new List<InvoiceItem>();
                var form = Request.Form;
                
                // Process regular inventory items
                for (int i = 0; i < 100; i++)
                {
                    var inventoryIdKey = $"InvoiceItems[{i}].InventoryId";
                    var quantityKey = $"InvoiceItems[{i}].Quantity";
                    var unitPriceKey = $"InvoiceItems[{i}].UnitPrice";
                    var totalKey = $"InvoiceItems[{i}].Total";
                    var itemNameKey = $"InvoiceItems[{i}].ItemName";
                    
                    if (!form.ContainsKey(inventoryIdKey) || string.IsNullOrEmpty(form[inventoryIdKey]))
                        break;
                    
                    if (int.TryParse(form[inventoryIdKey], out var inventoryId) && 
                        int.TryParse(form[quantityKey], out var quantity) &&
                        decimal.TryParse(form[unitPriceKey], out var unitPrice) &&
                        decimal.TryParse(form[totalKey], out var total))
                    {
                        // Get inventory item for name
                        var inventoryItem = await _inventoryService.GetInventoryByIdAsync(inventoryId);
                        var itemName = form[itemNameKey].ToString();
                        if (string.IsNullOrEmpty(itemName))
                        {
                            itemName = inventoryItem?.Name ?? "Unknown Item";
                        }
                        
                        var invoiceItem = new InvoiceItem
                        {
                            InvoiceId = 0, // Will be set by EF when invoice is saved
                            InventoryId = inventoryId,
                            ItemName = itemName,
                            Quantity = quantity,
                            UnitPrice = unitPrice,
                            Total = total,
                            Unit = inventoryItem?.Unit ?? "pcs"
                        };
                        
                        invoiceItems.Add(invoiceItem);
                    }
                }

                // Process job list items
                var includeJobListItems = form.ContainsKey("IncludeJobListItems") && form["IncludeJobListItems"].ToString() == "true";
                var includeCompletedJobs = form.ContainsKey("IncludeCompletedJobs") && form["IncludeCompletedJobs"].ToString() == "true";
                var includeAvailableJobs = form.ContainsKey("IncludeAvailableJobs") && form["IncludeAvailableJobs"].ToString() == "true";
                
                if (includeJobListItems)
                {
                    var selectedJobs = await _customerJobListService.GetSelectedCustomerJobListsAsync(invoice.CustomerId);
                    var jobIds = new List<int>();
                    foreach (var customerJob in selectedJobs)
                    {
                        var invoiceItem = new InvoiceItem
                        {
                            InvoiceId = 0,
                            ItemName = customerJob.JobList.JobName,
                            Description = customerJob.JobList.Description,
                            Quantity = 1,
                            UnitPrice = customerJob.JobList.Price,
                            Total = customerJob.JobList.Price,
                            Unit = "pcs"
                        };
                        invoiceItems.Add(invoiceItem);
                        jobIds.Add(customerJob.JobListId);
                    }
                    
                    // Mark selected jobs as invoiced (will be called after invoice is saved)
                    if (jobIds.Any())
                    {
                        TempData["JobIdsToMarkInvoiced"] = string.Join(",", jobIds);
                    }
                }
                
                if (includeCompletedJobs)
                {
                    var completedJobs = await _customerJobListService.GetCompletedCustomerJobListsAsync(invoice.CustomerId);
                    var jobIds = new List<int>();
                    foreach (var customerJob in completedJobs)
                    {
                        var invoiceItem = new InvoiceItem
                        {
                            InvoiceId = 0,
                            ItemName = customerJob.JobList.JobName + " (Completed)",
                            Description = customerJob.JobList.Description,
                            Quantity = 1,
                            UnitPrice = customerJob.JobList.Price,
                            Total = customerJob.JobList.Price,
                            Unit = "pcs"
                        };
                        
                        invoiceItems.Add(invoiceItem);
                        jobIds.Add(customerJob.JobListId);
                    }
                    
                    // Mark completed jobs as invoiced
                    if (jobIds.Any())
                    {
                        var existingJobIds = TempData["JobIdsToMarkInvoiced"]?.ToString() ?? "";
                        var allJobIds = existingJobIds + "," + string.Join(",", jobIds);
                        TempData["JobIdsToMarkInvoiced"] = allJobIds.TrimStart(',');
                    }
                }

                if (includeAvailableJobs)
                {
                    var allJobs = await _customerJobListService.GetCustomerJobListsAsync(invoice.CustomerId);
                    foreach (var customerJob in allJobs)
                    {
                        var invoiceItem = new InvoiceItem
                        {
                            InvoiceId = 0,
                            ItemName = customerJob.JobList.JobName,
                            Description = customerJob.JobList.Description,
                            Quantity = 1,
                            UnitPrice = customerJob.JobList.Price,
                            Total = customerJob.JobList.Price,
                            Unit = "pcs"
                        };
                        
                        invoiceItems.Add(invoiceItem);
                    }
                }

                invoice.InvoiceItems = invoiceItems;
                
                // Log invoice items count
                TempData["DebugMessage"] += $" | InvoiceItems.Count: {invoiceItems.Count}";
                
                // Set default values for required fields
                if (invoice.InvoiceDate == default)
                    invoice.InvoiceDate = DateTime.Now;
                if (invoice.DueDate == null)
                    invoice.DueDate = DateTime.Now.AddDays(7);
                
                // Validate required fields
                if (invoice.CustomerId == 0)
                {
                    ModelState.AddModelError("CustomerId", "Customer is required.");
                    TempData["DebugMessage"] += " | [ERROR] CustomerId is 0";
                }
                
                // Log ModelState errors
                if (!ModelState.IsValid)
                {
                    var errors = string.Join("; ", ModelState.Values.SelectMany(x => x.Errors).Select(x => x.ErrorMessage));
                    TempData["DebugMessage"] += $" | [ModelState Errors]: {errors}";
                }
                
                if (ModelState.IsValid && invoiceItems.Any())
                {
                    TempData["DebugMessage"] += " | [SUCCESS] ModelState valid, calling CreateInvoiceAsync.";
                    var createdInvoice = await _invoiceService.CreateInvoiceAsync(invoice);
                    
                    // Mark jobs as invoiced if any were included
                    var jobIdsToMark = TempData["JobIdsToMarkInvoiced"]?.ToString();
                    if (!string.IsNullOrEmpty(jobIdsToMark) && createdInvoice != null)
                    {
                        var jobIds = jobIdsToMark.Split(',').Select(int.Parse).ToList();
                        await _customerJobListService.MarkJobsAsInvoicedAsync(invoice.CustomerId, createdInvoice.Id, jobIds);
                    }
                    
                    // Auto-send WhatsApp if enabled
                    try
                    {
                        var configuration = HttpContext.RequestServices.GetService<IConfiguration>();
                        var appSettings = new Models.Configuration.AppSettings();
                        configuration?.GetSection("AppSettings").Bind(appSettings);
                        
                        if (appSettings.AutoSendWhatsAppOnInvoiceCreate && createdInvoice != null)
                        {
                            // Get customer details for WhatsApp
                            var customer = await _customerService.GetCustomerByIdAsync(createdInvoice.CustomerId);
                            if (customer != null && !string.IsNullOrEmpty(customer.Phone))
                            {
                                // Generate WhatsApp message
                                var message = await _whatsAppService.GenerateInvoiceMessageAsync(createdInvoice.Id);
                                
                                // Auto-send invoice with PDF via WhatsApp
                                var whatsAppResult = await _whatsAppService.SendInvoiceWithPdfAsync(createdInvoice.Id, customer.Phone, message);
                                if (whatsAppResult)
                                {
                                    TempData["SuccessMessage"] = "Invoice created and sent via WhatsApp successfully!";
                                }
                                else
                                {
                                    TempData["SuccessMessage"] = "Invoice created successfully! (WhatsApp sending failed)";
                                }
                            }
                            else
                            {
                                TempData["SuccessMessage"] = "Invoice created successfully! (Customer phone number not available for WhatsApp)";
                            }
                        }
                        else
                        {
                            TempData["SuccessMessage"] = "Invoice created successfully!";
                        }
                    }
                    catch (Exception whatsAppEx)
                    {
                        // Log WhatsApp error but don't fail the invoice creation
                        TempData["SuccessMessage"] = "Invoice created successfully! (WhatsApp sending failed)";
                        Console.WriteLine($"[InvoiceController] WhatsApp auto-send error: {whatsAppEx.Message}");
                    }
                    
                    return RedirectToAction(nameof(Index));
                }
                
                if (!invoiceItems.Any())
                {
                    ModelState.AddModelError("", "At least one invoice item is required.");
                    TempData["DebugMessage"] += " | [ERROR] No invoice items.";
                }
                if (!ModelState.IsValid)
                {
                    var errors = string.Join("; ", ModelState.Values.SelectMany(x => x.Errors).Select(x => x.ErrorMessage));
                    TempData["ErrorMessage"] = "Validation failed: " + errors;
                }
            }
            catch (Exception ex)
            {
                TempData["DebugMessage"] += $" | [EXCEPTION] {ex.Message}";
                TempData["ErrorMessage"] = $"Error creating invoice: {ex.Message}";
            }
            
            ViewBag.Customers = await _customerService.GetAllCustomersAsync();
            ViewBag.Inventory = await _inventoryService.GetAllActiveInventoryAsync();
            return View(invoice);
        }

        // GET: Invoice/Edit/5
        public async Task<IActionResult> Edit(int id)
        {
            var invoice = await _invoiceService.GetInvoiceByIdAsync(id);
            if (invoice == null)
                return NotFound();
            ViewBag.Customers = await _customerService.GetAllCustomersAsync();
            ViewBag.Inventory = await _inventoryService.GetAllActiveInventoryAsync();
            return View(invoice);
        }

        // POST: Invoice/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Invoice invoice)
        {
            if (id != invoice.Id)
                return NotFound();
            if (ModelState.IsValid)
            {
                await _invoiceService.UpdateInvoiceAsync(invoice);
                TempData["SuccessMessage"] = "Invoice updated successfully!";
                return RedirectToAction(nameof(Index));
            }
            ViewBag.Customers = await _customerService.GetAllCustomersAsync();
            ViewBag.Inventory = await _inventoryService.GetAllActiveInventoryAsync();
            return View(invoice);
        }

        // GET: Invoice/Delete/5
        public async Task<IActionResult> Delete(int id)
        {
            var invoice = await _invoiceService.GetInvoiceByIdAsync(id);
            if (invoice == null)
                return NotFound();
            return View(invoice);
        }

        // POST: Invoice/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var result = await _invoiceService.DeleteInvoiceAsync(id);
            if (result)
                TempData["SuccessMessage"] = "Invoice deleted successfully!";
            else
                TempData["ErrorMessage"] = "Failed to delete invoice.";
            return RedirectToAction(nameof(Index));
        }

        // POST: Invoice/SendEmail/5
        [HttpPost]
        public IActionResult SendEmail(int id)
        {
            // TODO: Implement email sending
            TempData["SuccessMessage"] = "Invoice sent via email (stub).";
            return RedirectToAction(nameof(Details), new { id });
        }

        // POST: Invoice/SendWhatsApp/5
        [HttpPost]
        public async Task<IActionResult> SendWhatsApp(int id)
        {
            try
            {
                var invoice = await _invoiceService.GetInvoiceByIdAsync(id);
                if (invoice == null)
                {
                    TempData["ErrorMessage"] = "Invoice not found.";
                    return RedirectToAction(nameof(Index));
                }

                // Get customer phone number
                var customerPhone = invoice.Customer?.Phone;
                if (string.IsNullOrEmpty(customerPhone))
                {
                    TempData["ErrorMessage"] = "Customer phone number not available.";
                    return RedirectToAction(nameof(Details), new { id });
                }

                // Validate phone number
                if (!await _whatsAppService.ValidatePhoneNumberAsync(customerPhone))
                {
                    TempData["ErrorMessage"] = "Invalid phone number format.";
                    return RedirectToAction(nameof(Details), new { id });
                }

                // Send invoice via WhatsApp
                var result = await _whatsAppService.SendInvoiceViaWhatsAppAsync(id, customerPhone);
                
                if (result)
                {
                    TempData["SuccessMessage"] = $"Invoice message sent successfully to {customerPhone}!";
                }
                else
                {
                    TempData["ErrorMessage"] = "Failed to send WhatsApp message. The message may have been prepared but not sent. Please check the browser window and send manually if needed.";
                }
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Error preparing WhatsApp message: {ex.Message}";
            }
            
            return RedirectToAction(nameof(Details), new { id });
        }

        // POST: Invoice/SendWhatsAppManual/5
        [HttpPost]
        public async Task<IActionResult> SendWhatsAppManual(int id)
        {
            try
            {
                var invoice = await _invoiceService.GetInvoiceByIdAsync(id);
                if (invoice == null)
                {
                    TempData["ErrorMessage"] = "Invoice not found.";
                    return RedirectToAction(nameof(Index));
                }

                // Get customer phone number
                var customerPhone = invoice.Customer?.Phone;
                if (string.IsNullOrEmpty(customerPhone))
                {
                    TempData["ErrorMessage"] = "Customer phone number not available.";
                    return RedirectToAction(nameof(Details), new { id });
                }

                // Validate phone number
                if (!await _whatsAppService.ValidatePhoneNumberAsync(customerPhone))
                {
                    TempData["ErrorMessage"] = "Invalid phone number format.";
                    return RedirectToAction(nameof(Details), new { id });
                }

                // Generate WhatsApp URL
                var message = await _whatsAppService.GenerateInvoiceMessageAsync(id);
                var formattedPhone = customerPhone.Replace(" ", "").Replace("-", "").Replace("(", "").Replace(")", "");
                if (formattedPhone.Length == 10)
                {
                    formattedPhone = "91" + formattedPhone;
                }
                
                var whatsappUrl = $"https://wa.me/{formattedPhone}?text={Uri.EscapeDataString(message)}";
                
                // Store URL in TempData for the view
                TempData["WhatsAppUrl"] = whatsappUrl;
                TempData["SuccessMessage"] = "WhatsApp URL generated. Click the link below to open WhatsApp.";
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Error generating WhatsApp URL: {ex.Message}";
            }
            
            return RedirectToAction(nameof(Details), new { id });
        }

        // POST: Invoice/SendWhatsAppWithPdf/5
        [HttpPost]
        public async Task<IActionResult> SendWhatsAppWithPdf(int id)
        {
            try
            {
                var invoice = await _invoiceService.GetInvoiceByIdAsync(id);
                if (invoice == null)
                {
                    TempData["ErrorMessage"] = "Invoice not found.";
                    return RedirectToAction(nameof(Index));
                }

                // Get customer phone number
                var customerPhone = invoice.Customer?.Phone;
                if (string.IsNullOrEmpty(customerPhone))
                {
                    TempData["ErrorMessage"] = "Customer phone number not available.";
                    return RedirectToAction(nameof(Details), new { id });
                }

                // Validate phone number
                if (!await _whatsAppService.ValidatePhoneNumberAsync(customerPhone))
                {
                    TempData["ErrorMessage"] = "Invalid phone number format.";
                    return RedirectToAction(nameof(Details), new { id });
                }

                // Send invoice PDF via WhatsApp
                var result = await _whatsAppService.SendInvoiceImageViaWhatsAppAsync(id, customerPhone);
                
                if (result)
                {
                    TempData["SuccessMessage"] = $"Invoice PDF sent successfully to {customerPhone} via WhatsApp!";
                }
                else
                {
                    TempData["ErrorMessage"] = "Failed to send invoice PDF via WhatsApp. Please check if WhatsApp Web is ready.";
                }
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Error sending invoice PDF via WhatsApp: {ex.Message}";
            }
            
            return RedirectToAction(nameof(Details), new { id });
        }

        // GET: Invoice/WhatsAppStatus
        [HttpGet]
        public IActionResult WhatsAppStatus()
        {
            return Json(new { 
                isReady = true, 
                message = "WhatsApp Web automation is available. Make sure to scan QR code when prompted." 
            });
        }

        // GET: Invoice/GetCompletedJobs/5
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

        // GET: Invoice/GetSelectedJobs/5
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

        // GET: Invoice/GetAvailableJobs/5
        [HttpGet]
        public async Task<IActionResult> GetAvailableJobs(int customerId)
        {
            var allJobs = await _customerJobListService.GetCustomerJobListsAsync(customerId);
            return Json(allJobs.Select(cjl => new
            {
                id = cjl.JobList.Id,
                jobName = cjl.JobList.JobName,
                description = cjl.JobList.Description,
                price = cjl.JobList.Price
            }));
        }

        // GET: Invoice/GetAllAvailableJobs/5
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
                    price = job.Price
                });
                
                return Json(jobs);
            }
            catch (Exception ex)
            {
                return Json(new { error = ex.Message });
            }
        }

        // GET: Invoice/TestPdf/5
        [HttpGet]
        public async Task<IActionResult> TestPdf(int id)
        {
            try
            {
                System.Diagnostics.Debug.WriteLine($"TestPdf called for invoice {id}");
                
                // Check if invoice exists
                var invoice = await _invoiceService.GetInvoiceByIdAsync(id);
                if (invoice == null)
                {
                    TempData["ErrorMessage"] = $"Invoice {id} not found";
                    return RedirectToAction(nameof(Details), new { id });
                }
                
                System.Diagnostics.Debug.WriteLine($"Invoice found: {invoice.InvoiceNumber}");
                
                var pdfBytes = await _pdfService.GenerateInvoicePdfAsync(id);
                System.Diagnostics.Debug.WriteLine($"PDF generation result: {pdfBytes?.Length ?? 0} bytes");
                
                if (pdfBytes != null && pdfBytes.Length > 0)
                {
                    TempData["SuccessMessage"] = $"PDF generated successfully! Size: {pdfBytes.Length} bytes";
                    System.Diagnostics.Debug.WriteLine($"PDF success: {pdfBytes.Length} bytes");
                }
                else
                {
                    TempData["ErrorMessage"] = "PDF generation failed - no bytes returned";
                    System.Diagnostics.Debug.WriteLine("PDF generation failed - no bytes returned");
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"TestPdf exception: {ex.Message}");
                System.Diagnostics.Debug.WriteLine($"Stack trace: {ex.StackTrace}");
                TempData["ErrorMessage"] = $"PDF generation error: {ex.Message}";
            }
            
            return RedirectToAction(nameof(Details), new { id });
        }
    }
} 