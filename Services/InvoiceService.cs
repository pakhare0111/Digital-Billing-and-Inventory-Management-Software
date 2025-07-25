using Billing_Software.Data;
using Billing_Software.Models;
using Microsoft.EntityFrameworkCore;

namespace Billing_Software.Services
{
    public class InvoiceService : IInvoiceService
    {
        private readonly ApplicationDbContext _context;

        public InvoiceService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Invoice>> GetAllInvoicesAsync()
        {
            return await _context.Invoices
                .Include(i => i.Customer)
                .Include(i => i.InvoiceItems)
                .OrderByDescending(i => i.CreatedDate)
                .ToListAsync();
        }

        public async Task<Invoice?> GetInvoiceByIdAsync(int id)
        {
            return await _context.Invoices
                .Include(i => i.Customer)
                .Include(i => i.InvoiceItems)
                .FirstOrDefaultAsync(i => i.Id == id);
        }

        public async Task<Invoice?> GetInvoiceByNumberAsync(string invoiceNumber)
        {
            return await _context.Invoices
                .Include(i => i.Customer)
                .Include(i => i.InvoiceItems)
                .FirstOrDefaultAsync(i => i.InvoiceNumber == invoiceNumber);
        }

        public async Task<Invoice> CreateInvoiceAsync(Invoice invoice)
        {
            Console.WriteLine($"[InvoiceService] CreateInvoiceAsync called. CustomerId: {invoice.CustomerId}, Items: {invoice.InvoiceItems?.Count}");
            invoice.CreatedDate = DateTime.Now;
            
            // Generate invoice number only if it's empty or temporary
            if (string.IsNullOrEmpty(invoice.InvoiceNumber) || invoice.InvoiceNumber.StartsWith("TEMP-"))
            {
                invoice.InvoiceNumber = await GenerateInvoiceNumberAsync();
            }
            
            // Calculate totals
            invoice.Subtotal = invoice.InvoiceItems?.Sum(item => item.Total) ?? 0;
            invoice.TaxAmount = invoice.Subtotal * 0.10m; // 10% tax
            invoice.Total = invoice.Subtotal + invoice.TaxAmount;
            invoice.Balance = invoice.Total - invoice.AmountPaid;
            
            // Set status based on total
            if (invoice.Total > 0)
            {
                invoice.Status = InvoiceStatus.Unpaid;
            }
            
            _context.Invoices.Add(invoice);
            await _context.SaveChangesAsync();
            Console.WriteLine($"[InvoiceService] Invoice saved. Id: {invoice.Id}, Number: {invoice.InvoiceNumber}");
            return invoice;
        }

        public async Task<Invoice> UpdateInvoiceAsync(Invoice invoice)
        {
            var existingInvoice = await _context.Invoices.FindAsync(invoice.Id);
            if (existingInvoice == null)
                throw new ArgumentException("Invoice not found");

            existingInvoice.CustomerId = invoice.CustomerId;
            existingInvoice.InvoiceDate = invoice.InvoiceDate;
            existingInvoice.DueDate = invoice.DueDate;
            existingInvoice.Notes = invoice.Notes;
            existingInvoice.Status = invoice.Status;
            existingInvoice.LastModified = DateTime.Now;

            await _context.SaveChangesAsync();
            return existingInvoice;
        }

        public async Task<bool> DeleteInvoiceAsync(int id)
        {
            var invoice = await _context.Invoices.FindAsync(id);
            if (invoice == null)
                return false;

            _context.Invoices.Remove(invoice);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<string> GenerateInvoiceNumberAsync()
        {
            var lastInvoice = await _context.Invoices
                .OrderByDescending(i => i.CreatedDate)
                .FirstOrDefaultAsync();

            if (lastInvoice == null)
                return "INV-0001";

            var lastNumber = int.Parse(lastInvoice.InvoiceNumber?.Split('-')[1] ?? "0");
            return $"INV-{(lastNumber + 1):D4}";
        }

        public decimal CalculateInvoiceTotalAsync(Invoice invoice)
        {
            var subtotal = invoice.InvoiceItems?.Sum(item => item.Total) ?? 0;
            var taxRate = 0.10m; // 10% tax rate
            var taxAmount = subtotal * taxRate;
            return subtotal + taxAmount;
        }

        public async Task<bool> SendInvoiceViaEmailAsync(int invoiceId, string recipientEmail)
        {
            // Implementation for email sending
            await Task.Delay(100); // Simulate email sending
            return true;
        }

        public async Task<bool> SendInvoiceViaWhatsAppAsync(int invoiceId, string recipientPhone)
        {
            // Implementation for WhatsApp sending
            await Task.Delay(100); // Simulate WhatsApp sending
            return true;
        }

        public async Task<byte[]> GenerateInvoicePdfAsync(int invoiceId)
        {
            // Implementation for PDF generation
            await Task.Delay(100); // Simulate PDF generation
            return new byte[0];
        }

        public async Task<IEnumerable<Invoice>> GetInvoicesByCustomerAsync(int customerId)
        {
            return await _context.Invoices
                .Include(i => i.Customer)
                .Where(i => i.CustomerId == customerId)
                .OrderByDescending(i => i.CreatedDate)
                .ToListAsync();
        }

        public async Task<IEnumerable<Invoice>> GetInvoicesByStatusAsync(InvoiceStatus status)
        {
            return await _context.Invoices
                .Include(i => i.Customer)
                .Where(i => i.Status == status)
                .OrderByDescending(i => i.CreatedDate)
                .ToListAsync();
        }

        public async Task<IEnumerable<Invoice>> GetOverdueInvoicesAsync()
        {
            return await _context.Invoices
                .Include(i => i.Customer)
                .Where(i => i.DueDate < DateTime.Today && i.Status != InvoiceStatus.Paid)
                .OrderBy(i => i.DueDate)
                .ToListAsync();
        }

        public async Task<IEnumerable<Invoice>> SearchInvoicesAsync(string searchTerm, string status)
        {
            var query = _context.Invoices
                .Include(i => i.Customer)
                .Include(i => i.InvoiceItems)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                query = query.Where(i => 
                    (i.InvoiceNumber != null && i.InvoiceNumber.Contains(searchTerm)) ||
                    (i.Customer != null && i.Customer.Name.Contains(searchTerm)) ||
                    (i.Customer != null && i.Customer.Phone.Contains(searchTerm)));
            }

            if (!string.IsNullOrWhiteSpace(status) && Enum.TryParse<InvoiceStatus>(status, out var statusEnum))
            {
                query = query.Where(i => i.Status == statusEnum);
            }

            return await query.OrderByDescending(i => i.CreatedDate).ToListAsync();
        }
    }
} 