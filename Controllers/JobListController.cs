using Microsoft.AspNetCore.Mvc;
using Billing_Software.Models;
using Billing_Software.Services;
using Microsoft.EntityFrameworkCore;
using OfficeOpenXml;
using System.Text;

namespace Billing_Software.Controllers
{
    public class JobListController : Controller
    {
        private readonly IJobListService _jobListService;

        public JobListController(IJobListService jobListService)
        {
            _jobListService = jobListService;
        }

        // GET: JobList
        public async Task<IActionResult> Index(string searchTerm)
        {
            ViewBag.SearchTerm = searchTerm;

            IEnumerable<JobList> jobLists;

            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                jobLists = await _jobListService.SearchJobListsAsync(searchTerm ?? "", null);
            }
            else
            {
                jobLists = await _jobListService.GetAllJobListsAsync();
            }

            return View(jobLists);
        }

        // GET: JobList/Details/5
        public async Task<IActionResult> Details(int id)
        {
            var jobList = await _jobListService.GetJobListByIdAsync(id);
            if (jobList == null)
            {
                return NotFound();
            }

            return View(jobList);
        }

        // GET: JobList/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: JobList/Create
        [HttpPost]
        public async Task<IActionResult> Create([Bind("JobName,Description,Price")] JobList jobList)
        {
            System.Diagnostics.Debug.WriteLine("=== CREATE JOB POST ACTION STARTED ===");
            
            // Debug: Log the received data
            System.Diagnostics.Debug.WriteLine($"JobName: {jobList.JobName}");
            System.Diagnostics.Debug.WriteLine($"Description: {jobList.Description}");
            System.Diagnostics.Debug.WriteLine($"Price: {jobList.Price}");
            System.Diagnostics.Debug.WriteLine($"ModelState.IsValid: {ModelState.IsValid}");
            
            // Temporarily bypass validation for testing
            if (string.IsNullOrWhiteSpace(jobList.JobName))
            {
                ModelState.AddModelError("JobName", "Job Name is required");
            }
            
            if (jobList.Price <= 0)
            {
                ModelState.AddModelError("Price", "Price must be greater than 0");
            }
            
            if (!ModelState.IsValid)
            {
                System.Diagnostics.Debug.WriteLine("=== MODEL STATE ERRORS ===");
                foreach (var error in ModelState.Values.SelectMany(v => v.Errors))
                {
                    System.Diagnostics.Debug.WriteLine($"Validation Error: {error.ErrorMessage}");
                }
                System.Diagnostics.Debug.WriteLine("=== END MODEL STATE ERRORS ===");
                return View(jobList);
            }
            
            try
            {
                System.Diagnostics.Debug.WriteLine("=== CREATING JOB ===");
                
                System.Diagnostics.Debug.WriteLine($"Final JobList - JobName: {jobList.JobName}, Description: {jobList.Description}, Price: {jobList.Price}");
                
                await _jobListService.CreateJobListAsync(jobList);
                
                System.Diagnostics.Debug.WriteLine("=== JOB CREATED SUCCESSFULLY ===");
                TempData["SuccessMessage"] = "Job created successfully!";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"=== ERROR CREATING JOB: {ex.Message} ===");
                TempData["ErrorMessage"] = $"Error creating job: {ex.Message}";
                return View(jobList);
            }
        }

        // GET: JobList/Edit/5
        public async Task<IActionResult> Edit(int id)
        {
            var jobList = await _jobListService.GetJobListByIdAsync(id);
            if (jobList == null)
            {
                return NotFound();
            }

            return View(jobList);
        }

        // POST: JobList/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,JobName,Description,Price,CreatedDate")] JobList jobList)
        {
            if (id != jobList.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    await _jobListService.UpdateJobListAsync(jobList);
                    TempData["SuccessMessage"] = "Job updated successfully!";
                    return RedirectToAction(nameof(Index));
                }
                catch (Exception ex)
                {
                    TempData["ErrorMessage"] = $"Error updating job: {ex.Message}";
                }
            }

            return View(jobList);
        }

        // GET: JobList/Delete/5
        public async Task<IActionResult> Delete(int id)
        {
            var jobList = await _jobListService.GetJobListByIdAsync(id);
            if (jobList == null)
            {
                return NotFound();
            }

            return View(jobList);
        }

        // POST: JobList/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var result = await _jobListService.DeleteJobListAsync(id);
            if (result)
            {
                TempData["SuccessMessage"] = "Job deleted successfully!";
            }
            else
            {
                TempData["ErrorMessage"] = "Failed to delete job.";
            }
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        public async Task<IActionResult> MarkAsCompleted(int id)
        {
            try
            {
                var jobList = await _jobListService.GetJobListByIdAsync(id);
                if (jobList == null)
                {
                    return Json(new { success = false, message = "Job not found." });
                }

                // Update job status to completed
                jobList.LastModified = DateTime.Now;
                await _jobListService.UpdateJobListAsync(jobList);

                return Json(new { success = true, message = "Job marked as completed." });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = $"Error: {ex.Message}" });
            }
        }

        [HttpPost]
        public async Task<IActionResult> MarkAsIncomplete(int id)
        {
            try
            {
                var jobList = await _jobListService.GetJobListByIdAsync(id);
                if (jobList == null)
                {
                    return Json(new { success = false, message = "Job not found." });
                }

                // Update job status to incomplete
                jobList.LastModified = DateTime.Now;
                await _jobListService.UpdateJobListAsync(jobList);

                return Json(new { success = true, message = "Job marked as incomplete." });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = $"Error: {ex.Message}" });
            }
        }

        // POST: JobList/ImportExcel
        [HttpPost]
        public async Task<IActionResult> ImportExcel(IFormFile excelFile)
        {
            try
            {
                if (excelFile == null || excelFile.Length == 0)
                {
                    return Json(new { success = false, message = "Please select a file to upload." });
                }

                if (excelFile.Length > 5 * 1024 * 1024) // 5MB limit
                {
                    return Json(new { success = false, message = "File size must be less than 5MB." });
                }

                var importedCount = 0;
                var errors = new List<string>();

                // Check file extension to determine format
                var fileExtension = Path.GetExtension(excelFile.FileName).ToLowerInvariant();
                var isCsv = fileExtension == ".csv";
                var isExcel = fileExtension == ".xlsx" || fileExtension == ".xls";

                if (!isCsv && !isExcel)
                {
                    return Json(new { success = false, message = "Please select a valid Excel (.xlsx, .xls) or CSV (.csv) file." });
                }

                using (var stream = new MemoryStream())
                {
                    await excelFile.CopyToAsync(stream);
                    stream.Position = 0;

                    if (isCsv)
                    {
                        // Handle CSV file
                        using (var reader = new StreamReader(stream))
                        {
                            var lineNumber = 0;
                            while (!reader.EndOfStream)
                            {
                                lineNumber++;
                                var line = await reader.ReadLineAsync();
                                if (line == null) continue;

                                // Skip header row
                                if (lineNumber == 1) continue;

                                try
                                {
                                    var values = ParseCsvLine(line);
                                    if (values.Length < 3)
                                    {
                                        errors.Add($"Row {lineNumber}: Insufficient columns. Expected 3, got {values.Length}");
                                        continue;
                                    }

                                    var jobName = values[0]?.Trim();
                                    var description = values[1]?.Trim();
                                    var priceStr = values[2]?.Trim();

                                    // Validate required fields
                                    if (string.IsNullOrWhiteSpace(jobName))
                                    {
                                        errors.Add($"Row {lineNumber}: Job Name is required.");
                                        continue;
                                    }

                                    // Parse numeric values
                                    if (!decimal.TryParse(priceStr, out var price) || price <= 0)
                                    {
                                        errors.Add($"Row {lineNumber}: Invalid price format. Must be greater than 0.");
                                        continue;
                                    }

                                    // Check if job name already exists
                                    var existingJob = await _jobListService.GetJobListByNameAsync(jobName);
                                    if (existingJob != null)
                                    {
                                        errors.Add($"Row {lineNumber}: Job '{jobName}' already exists.");
                                        continue;
                                    }

                                    // Create new job
                                    var jobList = new JobList
                                    {
                                        JobName = jobName,
                                        Description = description ?? "",
                                        Price = price,
                                        CreatedDate = DateTime.Now,
                                        LastModified = DateTime.Now
                                    };

                                    await _jobListService.CreateJobListAsync(jobList);
                                    importedCount++;
                                }
                                catch (Exception ex)
                                {
                                    errors.Add($"Row {lineNumber}: Error processing row - {ex.Message}");
                                }
                            }
                        }
                    }
                    else
                    {
                        // Handle Excel file
                        ExcelPackage.LicenseContext = LicenseContext.NonCommercial;

                        try
                        {
                            using (var package = new ExcelPackage(stream))
                            {
                                var worksheet = package.Workbook.Worksheets.FirstOrDefault();
                                if (worksheet == null)
                                {
                                    return Json(new { success = false, message = "No worksheet found in the Excel file." });
                                }

                                var rowCount = worksheet.Dimension?.Rows ?? 0;
                                if (rowCount < 2)
                                {
                                    return Json(new { success = false, message = "Excel file must have at least 2 rows (header + data)." });
                                }

                                // Skip header row, start from row 2
                                for (int row = 2; row <= rowCount; row++)
                                {
                                    try
                                    {
                                        var jobName = worksheet.Cells[row, 1].Value?.ToString()?.Trim();
                                        var description = worksheet.Cells[row, 2].Value?.ToString()?.Trim();
                                        var priceStr = worksheet.Cells[row, 3].Value?.ToString()?.Trim();

                                        // Validate required fields
                                        if (string.IsNullOrWhiteSpace(jobName))
                                        {
                                            errors.Add($"Row {row}: Job Name is required.");
                                            continue;
                                        }

                                        // Parse numeric values
                                        if (!decimal.TryParse(priceStr, out var price) || price <= 0)
                                        {
                                            errors.Add($"Row {row}: Invalid price format. Must be greater than 0.");
                                            continue;
                                        }

                                        // Check if job name already exists
                                        var existingJob = await _jobListService.GetJobListByNameAsync(jobName);
                                        if (existingJob != null)
                                        {
                                            errors.Add($"Row {row}: Job '{jobName}' already exists.");
                                            continue;
                                        }

                                        // Create new job
                                        var jobList = new JobList
                                        {
                                            JobName = jobName,
                                            Description = description ?? "",
                                            Price = price,
                                            CreatedDate = DateTime.Now,
                                            LastModified = DateTime.Now
                                        };

                                        await _jobListService.CreateJobListAsync(jobList);
                                        importedCount++;
                                    }
                                    catch (Exception ex)
                                    {
                                        errors.Add($"Row {row}: Error processing row - {ex.Message}");
                                    }
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            return Json(new { success = false, message = $"Excel file error: {ex.Message}. Please ensure the file is not corrupted or encrypted." });
                        }
                    }
                }

                var message = $"Successfully imported {importedCount} jobs.";
                if (errors.Any())
                {
                    message += $" {errors.Count} errors occurred: " + string.Join("; ", errors.Take(5));
                }

                return Json(new { success = true, importedCount = importedCount, message = message });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = $"Import failed: {ex.Message}" });
            }
        }

        private string[] ParseCsvLine(string line)
        {
            var result = new List<string>();
            var current = "";
            var inQuotes = false;
            
            for (int i = 0; i < line.Length; i++)
            {
                var ch = line[i];
                
                if (ch == '"')
                {
                    inQuotes = !inQuotes;
                }
                else if (ch == ',' && !inQuotes)
                {
                    result.Add(current);
                    current = "";
                }
                else
                {
                    current += ch;
                }
            }
            
            result.Add(current);
            return result.ToArray();
        }

        // GET: JobList/ExportExcel
        public async Task<IActionResult> ExportExcel()
        {
            try
            {
                var jobLists = await _jobListService.GetAllJobListsAsync();

                ExcelPackage.LicenseContext = LicenseContext.NonCommercial;

                using (var package = new ExcelPackage())
                {
                    var worksheet = package.Workbook.Worksheets.Add("JobList");

                    // Add headers
                    worksheet.Cells[1, 1].Value = "JobName";
                    worksheet.Cells[1, 2].Value = "Description";
                    worksheet.Cells[1, 3].Value = "Price";
                    worksheet.Cells[1, 4].Value = "CreatedDate";

                    // Style headers
                    using (var range = worksheet.Cells[1, 1, 1, 4])
                    {
                        range.Style.Font.Bold = true;
                        range.Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                        range.Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.LightGray);
                    }

                    // Add data
                    int row = 2;
                    foreach (var job in jobLists)
                    {
                        worksheet.Cells[row, 1].Value = job.JobName;
                        worksheet.Cells[row, 2].Value = job.Description;
                        worksheet.Cells[row, 3].Value = job.Price;
                        worksheet.Cells[row, 4].Value = job.CreatedDate.ToString("yyyy-MM-dd");
                        row++;
                    }

                    // Auto-fit columns
                    worksheet.Cells.AutoFitColumns();

                    // Convert to byte array
                    var content = package.GetAsByteArray();
                    var fileName = $"joblist_export_{DateTime.Now:yyyyMMdd_HHmmss}.xlsx";

                    return File(content, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
                }
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Export failed: {ex.Message}";
                return RedirectToAction(nameof(Index));
            }
        }

        // GET: JobList/CompletedJobs
        public async Task<IActionResult> CompletedJobs()
        {
            var completedJobs = await _jobListService.GetCompletedJobListsAsync();
            return View(completedJobs);
        }

        // GET: JobList/CustomerJobs
        public async Task<IActionResult> CustomerJobs()
        {
            var customerJobs = await _jobListService.GetCustomerJobListsAsync();
            return View(customerJobs);
        }
    }
} 