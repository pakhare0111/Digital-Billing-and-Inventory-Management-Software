namespace Billing_Software.Services
{
    public interface IEmailService
    {
        Task<bool> SendEmailAsync(string to, string subject, string body, string? attachmentPath = null);
        Task<bool> SendInvoiceEmailAsync(int invoiceId, string recipientEmail);
        Task<bool> SendServiceReminderEmailAsync(int serviceRecordId, string recipientEmail);
        Task<bool> SendLowStockAlertEmailAsync(string itemName, int currentStock, string recipientEmail);
        Task<bool> SendBulkEmailAsync(IEnumerable<string> recipients, string subject, string body);
        Task<string> GenerateInvoiceEmailTemplateAsync(int invoiceId);
        Task<string> GenerateServiceReminderTemplateAsync(int serviceRecordId);
    }
} 