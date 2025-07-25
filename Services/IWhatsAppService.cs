using Billing_Software.Models;

namespace Billing_Software.Services
{
    public interface IWhatsAppService
    {
        Task<bool> SendInvoiceViaWhatsAppAsync(int invoiceId, string recipientPhone);
        Task<bool> SendMessageViaWhatsAppAsync(string phoneNumber, string message);
        Task<bool> SendInvoiceWithPdfAsync(int invoiceId, string recipientPhone, string message);
        Task<bool> SendInvoiceImageViaWhatsAppAsync(int invoiceId, string recipientPhone);
        Task<string> GenerateInvoiceMessageAsync(int invoiceId);
        Task<bool> ValidatePhoneNumberAsync(string phoneNumber);
        Task<bool> SendBulkInvoiceAsync(List<int> invoiceIds, List<string> phoneNumbers);
    }
} 