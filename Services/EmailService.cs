using Billing_Software.Models.Configuration;
using Microsoft.Extensions.Options;

namespace Billing_Software.Services
{
    public class EmailService : IEmailService
    {
        private readonly EmailSettings _emailSettings;

        public EmailService(IOptions<EmailSettings> emailSettings)
        {
            _emailSettings = emailSettings.Value;
        }

        public async Task<bool> SendEmailAsync(string to, string subject, string body, string? attachmentPath = null)
        {
            try
            {
                // Implementation for sending email using SMTP
                // This is a placeholder implementation
                await Task.Delay(100); // Simulate email sending
                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> SendInvoiceEmailAsync(int invoiceId, string recipientEmail)
        {
            try
            {
                var subject = "Invoice Generated";
                var body = await GenerateInvoiceEmailTemplateAsync(invoiceId);
                return await SendEmailAsync(recipientEmail, subject, body);
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> SendServiceReminderEmailAsync(int serviceRecordId, string recipientEmail)
        {
            try
            {
                var subject = "Service Reminder";
                var body = await GenerateServiceReminderTemplateAsync(serviceRecordId);
                return await SendEmailAsync(recipientEmail, subject, body);
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> SendLowStockAlertEmailAsync(string itemName, int currentStock, string recipientEmail)
        {
            try
            {
                var subject = "Low Stock Alert";
                var body = $"The item '{itemName}' is running low on stock. Current quantity: {currentStock}";
                return await SendEmailAsync(recipientEmail, subject, body);
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> SendBulkEmailAsync(IEnumerable<string> recipients, string subject, string body)
        {
            try
            {
                var tasks = recipients.Select(recipient => SendEmailAsync(recipient, subject, body));
                var results = await Task.WhenAll(tasks);
                return results.All(result => result);
            }
            catch
            {
                return false;
            }
        }

        public async Task<string> GenerateInvoiceEmailTemplateAsync(int invoiceId)
        {
            // Implementation for generating invoice email template
            return await Task.FromResult($"Invoice #{invoiceId} has been generated. Please find the attached invoice.");
        }

        public async Task<string> GenerateServiceReminderTemplateAsync(int serviceRecordId)
        {
            // Implementation for generating service reminder template
            return await Task.FromResult($"Service reminder for record #{serviceRecordId}. Please schedule your service appointment.");
        }
    }
} 