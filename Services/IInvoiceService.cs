using Billing_Software.Models;

namespace Billing_Software.Services
{
    public interface IInvoiceService
    {
        Task<IEnumerable<Invoice>> GetAllInvoicesAsync();
        Task<Invoice?> GetInvoiceByIdAsync(int id);
        Task<Invoice?> GetInvoiceByNumberAsync(string invoiceNumber);
        Task<Invoice> CreateInvoiceAsync(Invoice invoice);
        Task<Invoice> UpdateInvoiceAsync(Invoice invoice);
        Task<bool> DeleteInvoiceAsync(int id);
        Task<string> GenerateInvoiceNumberAsync();
        decimal CalculateInvoiceTotalAsync(Invoice invoice);
        Task<bool> SendInvoiceViaEmailAsync(int invoiceId, string recipientEmail);
        Task<bool> SendInvoiceViaWhatsAppAsync(int invoiceId, string recipientPhone);
        Task<byte[]> GenerateInvoicePdfAsync(int invoiceId);
        Task<IEnumerable<Invoice>> GetInvoicesByCustomerAsync(int customerId);
        Task<IEnumerable<Invoice>> GetInvoicesByStatusAsync(InvoiceStatus status);
        Task<IEnumerable<Invoice>> GetOverdueInvoicesAsync();
        Task<IEnumerable<Invoice>> SearchInvoicesAsync(string searchTerm, string status);
    }
} 