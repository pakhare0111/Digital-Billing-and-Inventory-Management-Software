using Microsoft.AspNetCore.Mvc;
using Billing_Software.Models;
using Billing_Software.Services;
using Microsoft.EntityFrameworkCore;
using OfficeOpenXml;
using System.Text;

namespace Billing_Software.Controllers
{
    public class InventoryController : Controller
    {
        private readonly IInventoryService _inventoryService;

        public InventoryController(IInventoryService inventoryService)
        {
            _inventoryService = inventoryService;
        }

        // GET: Inventory
        public async Task<IActionResult> Index(string searchTerm, string category)
        {
            ViewBag.SearchTerm = searchTerm;
            ViewBag.Category = category;
            var inventory = await _inventoryService.SearchInventoryAsync(searchTerm);
            
            if (!string.IsNullOrWhiteSpace(category))
            {
                inventory = await _inventoryService.GetInventoryByCategoryAsync(category);
            }
            
            return View(inventory);
        }

        // GET: Inventory/Details/5
        public async Task<IActionResult> Details(int id)
        {
            var inventory = await _inventoryService.GetInventoryByIdAsync(id);
            if (inventory == null)
                return NotFound();
            return View(inventory);
        }

        // GET: Inventory/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Inventory/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Inventory inventory)
        {
            if (ModelState.IsValid)
            {
                await _inventoryService.CreateInventoryAsync(inventory);
                TempData["SuccessMessage"] = "Inventory item created successfully!";
                return RedirectToAction(nameof(Index));
            }
            return View(inventory);
        }

        // GET: Inventory/Edit/5
        public async Task<IActionResult> Edit(int id)
        {
            var inventory = await _inventoryService.GetInventoryByIdAsync(id);
            if (inventory == null)
                return NotFound();
            return View(inventory);
        }

        // POST: Inventory/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Inventory inventory)
        {
            if (id != inventory.Id)
                return NotFound();
            if (ModelState.IsValid)
            {
                await _inventoryService.UpdateInventoryAsync(inventory);
                TempData["SuccessMessage"] = "Inventory item updated successfully!";
                return RedirectToAction(nameof(Index));
            }
            return View(inventory);
        }

        // GET: Inventory/Delete/5
        public async Task<IActionResult> Delete(int id)
        {
            var inventory = await _inventoryService.GetInventoryByIdAsync(id);
            if (inventory == null)
                return NotFound();
            return View(inventory);
        }

        // POST: Inventory/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var result = await _inventoryService.DeleteInventoryAsync(id);
            if (result)
            {
                TempData["SuccessMessage"] = "Inventory item deleted successfully!";
            }
            else
            {
                TempData["ErrorMessage"] = "Failed to delete inventory item.";
            }
            return RedirectToAction(nameof(Index));
        }

        // GET: Inventory/LowStock
        public async Task<IActionResult> LowStock()
        {
            var lowStockItems = await _inventoryService.GetLowStockItemsAsync();
            return View(lowStockItems);
        }

        // GET: Inventory/OutOfStock
        public async Task<IActionResult> OutOfStock()
        {
            var outOfStockItems = await _inventoryService.GetOutOfStockItemsAsync();
            return View(outOfStockItems);
        }

        // GET: Inventory/StockValue
        public async Task<IActionResult> StockValue()
        {
            var stockValue = await _inventoryService.GetStockValueAsync();
            return View(stockValue);
        }

        // POST: Inventory/UpdateStock
        [HttpPost]
        public async Task<IActionResult> UpdateStock(int id, int quantity)
        {
            var result = await _inventoryService.UpdateStockAsync(id, quantity);
            return Json(new { success = result });
        }

        // POST: Inventory/ImportExcel
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
                                    if (values.Length < 7)
                                    {
                                        errors.Add($"Row {lineNumber}: Insufficient columns. Expected 7, got {values.Length}");
                                        continue;
                                    }

                                    var name = values[0]?.Trim();
                                    var sku = values[1]?.Trim();
                                    var description = values[2]?.Trim();
                                    var category = values[3]?.Trim();
                                    var priceStr = values[4]?.Trim();
                                    var quantityStr = values[5]?.Trim();
                                    var minStockStr = values[6]?.Trim();

                                    // Validate required fields
                                    if (string.IsNullOrWhiteSpace(name) || string.IsNullOrWhiteSpace(sku))
                                    {
                                        errors.Add($"Row {lineNumber}: Name and SKU are required.");
                                        continue;
                                    }

                                    // Parse numeric values
                                    if (!decimal.TryParse(priceStr, out var price))
                                    {
                                        errors.Add($"Row {lineNumber}: Invalid price format.");
                                        continue;
                                    }

                                    if (!int.TryParse(quantityStr, out var quantity))
                                    {
                                        errors.Add($"Row {lineNumber}: Invalid quantity format.");
                                        continue;
                                    }

                                    if (!int.TryParse(minStockStr, out var minStock))
                                    {
                                        errors.Add($"Row {lineNumber}: Invalid minimum stock format.");
                                        continue;
                                    }

                                    // Check if SKU already exists
                                    var existingItem = await _inventoryService.GetInventoryBySkuAsync(sku);
                                    if (existingItem != null)
                                    {
                                        errors.Add($"Row {lineNumber}: SKU '{sku}' already exists.");
                                        continue;
                                    }

                                    // Create new inventory item
                                    var inventory = new Inventory
                                    {
                                        Name = name,
                                        SKU = sku,
                                        Description = description ?? "",
                                        Category = category ?? "General",
                                        Price = price,
                                        Quantity = quantity,
                                        MinStockLevel = minStock,
                                        CreatedDate = DateTime.Now,
                                        LastUpdated = DateTime.Now
                                    };

                                    await _inventoryService.CreateInventoryAsync(inventory);
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
                                        var name = worksheet.Cells[row, 1].Value?.ToString()?.Trim();
                                        var sku = worksheet.Cells[row, 2].Value?.ToString()?.Trim();
                                        var description = worksheet.Cells[row, 3].Value?.ToString()?.Trim();
                                        var category = worksheet.Cells[row, 4].Value?.ToString()?.Trim();
                                        var priceStr = worksheet.Cells[row, 5].Value?.ToString()?.Trim();
                                        var quantityStr = worksheet.Cells[row, 6].Value?.ToString()?.Trim();
                                        var minStockStr = worksheet.Cells[row, 7].Value?.ToString()?.Trim();

                                        // Validate required fields
                                        if (string.IsNullOrWhiteSpace(name) || string.IsNullOrWhiteSpace(sku))
                                        {
                                            errors.Add($"Row {row}: Name and SKU are required.");
                                            continue;
                                        }

                                        // Parse numeric values
                                        if (!decimal.TryParse(priceStr, out var price))
                                        {
                                            errors.Add($"Row {row}: Invalid price format.");
                                            continue;
                                        }

                                        if (!int.TryParse(quantityStr, out var quantity))
                                        {
                                            errors.Add($"Row {row}: Invalid quantity format.");
                                            continue;
                                        }

                                        if (!int.TryParse(minStockStr, out var minStock))
                                        {
                                            errors.Add($"Row {row}: Invalid minimum stock format.");
                                            continue;
                                        }

                                        // Check if SKU already exists
                                        var existingItem = await _inventoryService.GetInventoryBySkuAsync(sku);
                                        if (existingItem != null)
                                        {
                                            errors.Add($"Row {row}: SKU '{sku}' already exists.");
                                            continue;
                                        }

                                        // Create new inventory item
                                        var inventory = new Inventory
                                        {
                                            Name = name,
                                            SKU = sku,
                                            Description = description ?? "",
                                            Category = category ?? "General",
                                            Price = price,
                                            Quantity = quantity,
                                            MinStockLevel = minStock,
                                            CreatedDate = DateTime.Now,
                                            LastUpdated = DateTime.Now
                                        };

                                        await _inventoryService.CreateInventoryAsync(inventory);
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

                var message = $"Successfully imported {importedCount} items.";
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

        // GET: Inventory/ExportExcel
        public async Task<IActionResult> ExportExcel()
        {
            try
            {
                var inventory = await _inventoryService.GetAllInventoryAsync();

                ExcelPackage.LicenseContext = LicenseContext.NonCommercial;

                using (var package = new ExcelPackage())
                {
                    var worksheet = package.Workbook.Worksheets.Add("Inventory");

                    // Add headers
                    worksheet.Cells[1, 1].Value = "Name";
                    worksheet.Cells[1, 2].Value = "SKU";
                    worksheet.Cells[1, 3].Value = "Description";
                    worksheet.Cells[1, 4].Value = "Category";
                    worksheet.Cells[1, 5].Value = "Price";
                    worksheet.Cells[1, 6].Value = "Quantity";
                    worksheet.Cells[1, 7].Value = "MinStockLevel";
                    worksheet.Cells[1, 8].Value = "CreatedDate";

                    // Style headers
                    using (var range = worksheet.Cells[1, 1, 1, 8])
                    {
                        range.Style.Font.Bold = true;
                        range.Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                        range.Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.LightGray);
                    }

                    // Add data
                    int row = 2;
                    foreach (var item in inventory)
                    {
                        worksheet.Cells[row, 1].Value = item.Name;
                        worksheet.Cells[row, 2].Value = item.SKU;
                        worksheet.Cells[row, 3].Value = item.Description;
                        worksheet.Cells[row, 4].Value = item.Category;
                        worksheet.Cells[row, 5].Value = item.Price;
                        worksheet.Cells[row, 6].Value = item.Quantity;
                        worksheet.Cells[row, 7].Value = item.MinStockLevel;
                        worksheet.Cells[row, 8].Value = item.CreatedDate.ToString("yyyy-MM-dd");
                        row++;
                    }

                    // Auto-fit columns
                    worksheet.Cells.AutoFitColumns();

                    // Convert to byte array
                    var content = package.GetAsByteArray();
                    var fileName = $"inventory_export_{DateTime.Now:yyyyMMdd_HHmmss}.xlsx";

                    return File(content, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
                }
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Export failed: {ex.Message}";
                return RedirectToAction(nameof(Index));
            }
        }
    }
} 