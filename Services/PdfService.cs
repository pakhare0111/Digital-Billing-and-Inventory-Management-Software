using Billing_Software.Data;
using Billing_Software.Models;
using Microsoft.EntityFrameworkCore;
using iTextSharp.text;
using iTextSharp.text.pdf;
using System.IO;

namespace Billing_Software.Services
{
    public class PdfService : IPdfService
    {
        private readonly ApplicationDbContext _context;

        public PdfService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<byte[]> GenerateInvoicePdfAsync(int invoiceId)
        {
            try
            {
                System.Diagnostics.Debug.WriteLine($"Starting PDF generation for invoice {invoiceId}");
                
                var invoice = await _context.Invoices
                    .Include(i => i.Customer)
                    .Include(i => i.InvoiceItems)
                    .FirstOrDefaultAsync(i => i.Id == invoiceId);

                if (invoice == null)
                {
                    System.Diagnostics.Debug.WriteLine("Invoice not found");
                    return new byte[0];
                }

                System.Diagnostics.Debug.WriteLine($"Found invoice: {invoice.InvoiceNumber}");

                // Create Invoices directory if it doesn't exist
                var invoicesDir = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "Invoices");
                if (!Directory.Exists(invoicesDir))
                {
                    Directory.CreateDirectory(invoicesDir);
                }

                // Generate PDF file path
                var fileName = $"Invoice_{invoiceId}_{DateTime.Now:yyyyMMdd_HHmmss}.pdf";
                var filePath = Path.Combine(invoicesDir, fileName);

                using (FileStream fs = new FileStream(filePath, FileMode.Create))
                {
                    Document document = new Document(PageSize.A4, 25, 25, 30, 30);
                    PdfWriter writer = PdfWriter.GetInstance(document, fs);
                    
                    document.Open();
                    
                    // Add title
                    Font titleFont = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 18);
                    Paragraph title = new Paragraph("INVOICE", titleFont);
                    title.Alignment = Element.ALIGN_CENTER;
                    document.Add(title);
                    document.Add(new Paragraph(" ")); // Spacing
                    
                    // Add invoice details
                    Font normalFont = FontFactory.GetFont(FontFactory.HELVETICA, 12);
                    Font boldFont = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 12);
                    
                    document.Add(new Paragraph($"Invoice Number: {invoice.InvoiceNumber}", boldFont));
                    document.Add(new Paragraph($"Date: {invoice.InvoiceDate:MMM dd, yyyy}", normalFont));
                    document.Add(new Paragraph($"Due Date: {invoice.DueDate:MMM dd, yyyy}", normalFont));
                    document.Add(new Paragraph(" ")); // Spacing
                    
                    // Add customer information
                    document.Add(new Paragraph("CUSTOMER INFORMATION:", boldFont));
                    document.Add(new Paragraph($"Name: {invoice.Customer?.Name}", normalFont));
                    document.Add(new Paragraph($"Address: {invoice.Customer?.Address}", normalFont));
                    document.Add(new Paragraph($"Phone: {invoice.Customer?.Phone}", normalFont));
                    document.Add(new Paragraph($"Email: {invoice.Customer?.Email}", normalFont));
                    document.Add(new Paragraph(" ")); // Spacing
                    
                    // Create items table
                    PdfPTable table = new PdfPTable(5);
                    table.WidthPercentage = 100;
                    
                    // Add headers
                    table.AddCell(new PdfPCell(new Phrase("Item", boldFont)));
                    table.AddCell(new PdfPCell(new Phrase("Description", boldFont)));
                    table.AddCell(new PdfPCell(new Phrase("Qty", boldFont)));
                    table.AddCell(new PdfPCell(new Phrase("Price", boldFont)));
                    table.AddCell(new PdfPCell(new Phrase("Total", boldFont)));
                    
                    // Add items
                    foreach (var item in invoice.InvoiceItems)
                    {
                        table.AddCell(new PdfPCell(new Phrase(item.ItemName, normalFont)));
                        table.AddCell(new PdfPCell(new Phrase(item.Description, normalFont)));
                        table.AddCell(new PdfPCell(new Phrase(item.Quantity.ToString(), normalFont)));
                        table.AddCell(new PdfPCell(new Phrase($"${item.UnitPrice:F2}", normalFont)));
                        table.AddCell(new PdfPCell(new Phrase($"${item.Total:F2}", normalFont)));
                    }
                    
                    document.Add(table);
                    document.Add(new Paragraph(" ")); // Spacing
                    
                    // Add totals
                    document.Add(new Paragraph($"Subtotal: ${invoice.Subtotal:F2}", boldFont));
                    document.Add(new Paragraph($"Tax: ${invoice.TaxAmount:F2}", normalFont));
                    document.Add(new Paragraph($"Total: ${invoice.Total:F2}", boldFont));
                    document.Add(new Paragraph(" ")); // Spacing
                    
                    // Add notes
                    if (!string.IsNullOrEmpty(invoice.Notes))
                    {
                        document.Add(new Paragraph("Notes:", boldFont));
                        document.Add(new Paragraph(invoice.Notes, normalFont));
                    }
                    
                    document.Close();
                }

                // Read the file back to return as bytes
                byte[] pdfBytes = await File.ReadAllBytesAsync(filePath);
                System.Diagnostics.Debug.WriteLine($"PDF generated successfully, size: {pdfBytes.Length} bytes, saved to: {filePath}");
                return pdfBytes;
            }
            catch (Exception ex)
            {
                // Log the exception for debugging
                System.Diagnostics.Debug.WriteLine($"PDF Generation Error: {ex.Message}");
                System.Diagnostics.Debug.WriteLine($"Stack trace: {ex.StackTrace}");
                return new byte[0];
            }
        }

        public async Task<string> GenerateInvoicePdfFileAsync(int invoiceId)
        {
            try
            {
                System.Diagnostics.Debug.WriteLine($"Starting PDF file generation for invoice {invoiceId}");
                
                var invoice = await _context.Invoices
                    .Include(i => i.Customer)
                    .Include(i => i.InvoiceItems)
                    .FirstOrDefaultAsync(i => i.Id == invoiceId);

                if (invoice == null)
                {
                    System.Diagnostics.Debug.WriteLine("Invoice not found");
                    return string.Empty;
                }

                System.Diagnostics.Debug.WriteLine($"Found invoice: {invoice.InvoiceNumber}");

                // Create Invoices directory if it doesn't exist
                var invoicesDir = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "Invoices");
                if (!Directory.Exists(invoicesDir))
                {
                    Directory.CreateDirectory(invoicesDir);
                }

                // Generate PDF file path
                var fileName = $"Invoice_{invoiceId}_{DateTime.Now:yyyyMMdd_HHmmss}.pdf";
                var filePath = Path.Combine(invoicesDir, fileName);

                using (FileStream fs = new FileStream(filePath, FileMode.Create))
                {
                    Document document = new Document(PageSize.A4, 25, 25, 30, 30);
                    PdfWriter writer = PdfWriter.GetInstance(document, fs);
                    
                    document.Open();
                    
                    // Add title
                    Font titleFont = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 18);
                    Paragraph title = new Paragraph("INVOICE", titleFont);
                    title.Alignment = Element.ALIGN_CENTER;
                    document.Add(title);
                    document.Add(new Paragraph(" ")); // Spacing
                    
                    // Add invoice details
                    Font normalFont = FontFactory.GetFont(FontFactory.HELVETICA, 12);
                    Font boldFont = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 12);
                    
                    document.Add(new Paragraph($"Invoice Number: {invoice.InvoiceNumber}", boldFont));
                    document.Add(new Paragraph($"Date: {invoice.InvoiceDate:MMM dd, yyyy}", normalFont));
                    document.Add(new Paragraph($"Due Date: {invoice.DueDate:MMM dd, yyyy}", normalFont));
                    document.Add(new Paragraph(" ")); // Spacing
                    
                    // Add customer information
                    document.Add(new Paragraph("CUSTOMER INFORMATION:", boldFont));
                    document.Add(new Paragraph($"Name: {invoice.Customer?.Name}", normalFont));
                    document.Add(new Paragraph($"Address: {invoice.Customer?.Address}", normalFont));
                    document.Add(new Paragraph($"Phone: {invoice.Customer?.Phone}", normalFont));
                    document.Add(new Paragraph($"Email: {invoice.Customer?.Email}", normalFont));
                    document.Add(new Paragraph(" ")); // Spacing
                    
                    // Create items table
                    PdfPTable table = new PdfPTable(5);
                    table.WidthPercentage = 100;
                    
                    // Add headers
                    table.AddCell(new PdfPCell(new Phrase("Item", boldFont)));
                    table.AddCell(new PdfPCell(new Phrase("Description", boldFont)));
                    table.AddCell(new PdfPCell(new Phrase("Qty", boldFont)));
                    table.AddCell(new PdfPCell(new Phrase("Price", boldFont)));
                    table.AddCell(new PdfPCell(new Phrase("Total", boldFont)));
                    
                    // Add items
                    foreach (var item in invoice.InvoiceItems)
                    {
                        table.AddCell(new PdfPCell(new Phrase(item.ItemName, normalFont)));
                        table.AddCell(new PdfPCell(new Phrase(item.Description, normalFont)));
                        table.AddCell(new PdfPCell(new Phrase(item.Quantity.ToString(), normalFont)));
                        table.AddCell(new PdfPCell(new Phrase($"${item.UnitPrice:F2}", normalFont)));
                        table.AddCell(new PdfPCell(new Phrase($"${item.Total:F2}", normalFont)));
                    }
                    
                    document.Add(table);
                    document.Add(new Paragraph(" ")); // Spacing
                    
                    // Add totals
                    document.Add(new Paragraph($"Subtotal: ${invoice.Subtotal:F2}", boldFont));
                    document.Add(new Paragraph($"Tax: ${invoice.TaxAmount:F2}", normalFont));
                    document.Add(new Paragraph($"Total: ${invoice.Total:F2}", boldFont));
                    document.Add(new Paragraph(" ")); // Spacing
                    
                    // Add notes
                    if (!string.IsNullOrEmpty(invoice.Notes))
                    {
                        document.Add(new Paragraph("Notes:", boldFont));
                        document.Add(new Paragraph(invoice.Notes, normalFont));
                    }
                    
                    document.Close();
                }

                System.Diagnostics.Debug.WriteLine($"PDF file generated successfully, saved to: {filePath}");
                return filePath;
            }
            catch (Exception ex)
            {
                // Log the exception for debugging
                System.Diagnostics.Debug.WriteLine($"PDF File Generation Error: {ex.Message}");
                System.Diagnostics.Debug.WriteLine($"Stack trace: {ex.StackTrace}");
                return string.Empty;
            }
        }

        public async Task<byte[]> GenerateServiceReportPdfAsync(int serviceRecordId)
        {
            try
            {
                var serviceRecord = await _context.ServiceRecords
                    .Include(s => s.Customer)
                    .Include(s => s.Vehicle)
                    .FirstOrDefaultAsync(s => s.Id == serviceRecordId);

                if (serviceRecord == null)
                    return new byte[0];

                var html = await GenerateServiceReportHtmlAsync(serviceRecordId);
                
                // Implementation for converting HTML to PDF
                await Task.Delay(100); // Simulate PDF generation
                
                return new byte[0]; // Placeholder return
            }
            catch
            {
                return new byte[0];
            }
        }

        public async Task<byte[]> GenerateCustomerReportPdfAsync(int customerId)
        {
            try
            {
                var customer = await _context.Customers
                    .Include(c => c.Invoices)
                    .Include(c => c.ServiceRecords)
                    .Include(c => c.Vehicles)
                    .FirstOrDefaultAsync(c => c.Id == customerId);

                if (customer == null)
                    return new byte[0];

                // Implementation for generating customer report PDF
                await Task.Delay(100); // Simulate PDF generation
                
                return new byte[0]; // Placeholder return
            }
            catch
            {
                return new byte[0];
            }
        }

        public async Task<byte[]> GenerateInventoryReportPdfAsync()
        {
            try
            {
                var inventory = await _context.Inventories
                    .Where(i => i.IsActive)
                    .ToListAsync();

                // Implementation for generating inventory report PDF
                await Task.Delay(100); // Simulate PDF generation
                
                return new byte[0]; // Placeholder return
            }
            catch
            {
                return new byte[0];
            }
        }

        public async Task<string> GenerateInvoiceHtmlAsync(int invoiceId)
        {
            var invoice = await _context.Invoices
                .Include(i => i.Customer)
                .Include(i => i.InvoiceItems)
                .FirstOrDefaultAsync(i => i.Id == invoiceId);

            if (invoice == null)
                return string.Empty;

            var html = $@"
                <html>
                <head>
                    <style>
                        body {{ font-family: Arial, sans-serif; margin: 20px; }}
                        .header {{ text-align: center; margin-bottom: 30px; }}
                        .invoice-details {{ margin-bottom: 20px; }}
                        .customer-info {{ margin-bottom: 20px; }}
                        table {{ width: 100%; border-collapse: collapse; }}
                        th, td {{ border: 1px solid #ddd; padding: 8px; text-align: left; }}
                        th {{ background-color: #f2f2f2; }}
                        .total {{ font-weight: bold; text-align: right; }}
                    </style>
                </head>
                <body>
                    <div class='header'>
                        <h1>INVOICE</h1>
                        <h2>{invoice.InvoiceNumber}</h2>
                    </div>
                    
                    <div class='invoice-details'>
                        <p><strong>Date:</strong> {invoice.InvoiceDate:MMM dd, yyyy}</p>
                        <p><strong>Due Date:</strong> {invoice.DueDate:MMM dd, yyyy}</p>
                    </div>
                    
                    <div class='customer-info'>
                        <h3>Bill To:</h3>
                        <p><strong>{invoice.Customer?.Name}</strong></p>
                        <p>{invoice.Customer?.Address}</p>
                        <p>Phone: {invoice.Customer?.Phone}</p>
                        <p>Email: {invoice.Customer?.Email}</p>
                    </div>
                    
                    <table>
                        <thead>
                            <tr>
                                <th>Item</th>
                                <th>Description</th>
                                <th>Qty</th>
                                <th>Price</th>
                                <th>Total</th>
                            </tr>
                        </thead>
                        <tbody>";

            foreach (var item in invoice.InvoiceItems)
            {
                html += $@"
                            <tr>
                                <td>{item.ItemName}</td>
                                <td>{item.Description}</td>
                                <td>{item.Quantity}</td>
                                <td>${item.UnitPrice:F2}</td>
                                <td>${item.Total:F2}</td>
                            </tr>";
            }

            html += $@"
                        </tbody>
                    </table>
                    
                    <div class='total'>
                        <p><strong>Subtotal:</strong> ${invoice.Subtotal:F2}</p>
                        <p><strong>Tax:</strong> ${invoice.TaxAmount:F2}</p>
                        <p><strong>Total:</strong> ${invoice.Total:F2}</p>
                    </div>
                    
                    <div style='margin-top: 30px;'>
                        <p><strong>Notes:</strong></p>
                        <p>{invoice.Notes}</p>
                    </div>
                </body>
                </html>";

            return html;
        }

        public async Task<string> GenerateServiceReportHtmlAsync(int serviceRecordId)
        {
            var serviceRecord = await _context.ServiceRecords
                .Include(s => s.Customer)
                .Include(s => s.Vehicle)
                .FirstOrDefaultAsync(s => s.Id == serviceRecordId);

            if (serviceRecord == null)
                return string.Empty;

            var html = $@"
                <html>
                <head>
                    <style>
                        body {{ font-family: Arial, sans-serif; margin: 20px; }}
                        .header {{ text-align: center; margin-bottom: 30px; }}
                        .service-details {{ margin-bottom: 20px; }}
                        .customer-info {{ margin-bottom: 20px; }}
                        .vehicle-info {{ margin-bottom: 20px; }}
                    </style>
                </head>
                <body>
                    <div class='header'>
                        <h1>SERVICE REPORT</h1>
                        <h2>Service #{serviceRecord.Id}</h2>
                    </div>
                    
                    <div class='service-details'>
                        <h3>Service Details:</h3>
                        <p><strong>Service Type:</strong> {serviceRecord.ServiceType}</p>
                        <p><strong>Service Date:</strong> {serviceRecord.ServiceDate:MMM dd, yyyy}</p>
                        <p><strong>Next Service Date:</strong> {serviceRecord.NextServiceDate:MMM dd, yyyy}</p>
                        <p><strong>Status:</strong> {serviceRecord.Status}</p>
                        <p><strong>Technician:</strong> {serviceRecord.Technician}</p>
                        <p><strong>Cost:</strong> ${serviceRecord.Cost:F2}</p>
                    </div>
                    
                    <div class='customer-info'>
                        <h3>Customer Information:</h3>
                        <p><strong>Name:</strong> {serviceRecord.Customer?.Name}</p>
                        <p><strong>Phone:</strong> {serviceRecord.Customer?.Phone}</p>
                        <p><strong>Email:</strong> {serviceRecord.Customer?.Email}</p>
                    </div>
                    
                    <div class='vehicle-info'>
                        <h3>Vehicle Information:</h3>
                        <p><strong>Brand:</strong> {serviceRecord.Vehicle?.Brand}</p>
                        <p><strong>Model:</strong> {serviceRecord.Vehicle?.Model}</p>
                        <p><strong>Year:</strong> {serviceRecord.Vehicle?.Year}</p>
                        <p><strong>Plate Number:</strong> {serviceRecord.Vehicle?.PlateNumber}</p>
                    </div>
                    
                    <div style='margin-top: 30px;'>
                        <h3>Service Notes:</h3>
                        <p>{serviceRecord.Notes}</p>
                    </div>
                </body>
                </html>";

            return html;
        }
    }
} 