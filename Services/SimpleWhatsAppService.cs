using Billing_Software.Data;
using Billing_Software.Models;
using Microsoft.EntityFrameworkCore;
using System.Text;

namespace Billing_Software.Services
{
    public class SimpleWhatsAppService : IWhatsAppService
    {
        private readonly ApplicationDbContext _context;
        private readonly IInvoiceService _invoiceService;
        private readonly IPdfService _pdfService;
        private readonly ILogger<SimpleWhatsAppService> _logger;

        public SimpleWhatsAppService(
            ApplicationDbContext context,
            IInvoiceService invoiceService,
            IPdfService pdfService,
            ILogger<SimpleWhatsAppService> logger)
        {
            _context = context;
            _invoiceService = invoiceService;
            _pdfService = pdfService;
            _logger = logger;
        }

        public async Task<bool> SendInvoiceViaWhatsAppAsync(int invoiceId, string recipientPhone)
        {
            try
            {
                var invoice = await _invoiceService.GetInvoiceByIdAsync(invoiceId);
                if (invoice == null)
                {
                    _logger.LogError($"Invoice {invoiceId} not found");
                    return false;
                }

                // Format phone number
                var formattedPhone = FormatPhoneNumber(recipientPhone);
                
                // Generate invoice message
                var message = await GenerateInvoiceMessageAsync(invoiceId);
                
                // Create WhatsApp URL
                var whatsappUrl = CreateWhatsAppUrl(formattedPhone, message);
                
                // Log the action
                await LogNotificationAsync(invoiceId, formattedPhone, message, NotificationStatus.Pending);
                
                // Return the URL for manual opening
                _logger.LogInformation($"WhatsApp URL generated for invoice {invoiceId}: {whatsappUrl}");
                
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error generating WhatsApp URL for invoice {invoiceId}: {ex.Message}");
                await LogNotificationAsync(invoiceId, recipientPhone, "", NotificationStatus.Failed);
                return false;
            }
        }

        public async Task<bool> SendMessageViaWhatsAppAsync(string phoneNumber, string message)
        {
            try
            {
                var formattedPhone = FormatPhoneNumber(phoneNumber);
                var whatsappUrl = CreateWhatsAppUrl(formattedPhone, message);
                
                _logger.LogInformation($"WhatsApp URL generated: {whatsappUrl}");
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error generating WhatsApp URL: {ex.Message}");
                return false;
            }
        }

        public async Task<bool> SendInvoiceWithPdfAsync(int invoiceId, string recipientPhone, string message)
        {
            try
            {
                var invoice = await _invoiceService.GetInvoiceByIdAsync(invoiceId);
                if (invoice == null)
                {
                    return false;
                }

                var formattedPhone = FormatPhoneNumber(recipientPhone);
                
                // For manual method, we'll just generate the message URL
                // The PDF would need to be manually attached
                var whatsappUrl = CreateWhatsAppUrl(formattedPhone, message);
                
                _logger.LogInformation($"WhatsApp URL with PDF message generated: {whatsappUrl}");
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error generating WhatsApp PDF URL: {ex.Message}");
                return false;
            }
        }

        public async Task<bool> SendInvoiceImageViaWhatsAppAsync(int invoiceId, string recipientPhone)
        {
            try
            {
                var invoice = await _invoiceService.GetInvoiceByIdAsync(invoiceId);
                if (invoice == null)
                {
                    return false;
                }

                var formattedPhone = FormatPhoneNumber(recipientPhone);
                var message = await GenerateInvoiceMessageAsync(invoiceId);
                
                // For PDF, we'll just generate the message URL
                // The PDF would need to be manually attached
                var whatsappUrl = CreateWhatsAppUrl(formattedPhone, message);
                
                _logger.LogInformation($"WhatsApp URL with PDF message generated: {whatsappUrl}");
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error generating WhatsApp PDF URL: {ex.Message}");
                return false;
            }
        }

        public async Task<string> GenerateInvoiceMessageAsync(int invoiceId)
        {
            var invoice = await _invoiceService.GetInvoiceByIdAsync(invoiceId);
            if (invoice == null)
                return string.Empty;

            var sb = new StringBuilder();
            sb.AppendLine($"🏢 *{invoice.Customer?.Name}*");
            sb.AppendLine($"📄 Invoice: {invoice.InvoiceNumber}");
            sb.AppendLine($"📅 Date: {invoice.InvoiceDate:dd/MM/yyyy}");
            sb.AppendLine($"💰 Total: ${invoice.Total:F2}");
            sb.AppendLine($"📋 Status: {invoice.Status}");
            sb.AppendLine();
            sb.AppendLine("📋 *Items:*");
            
            foreach (var item in invoice.InvoiceItems)
            {
                sb.AppendLine($"• {item.ItemName} x{item.Quantity} = ${item.Total:F2}");
            }
            
            sb.AppendLine();
            sb.AppendLine("Thank you for your business! 🚗");
            sb.AppendLine("Please contact us for any questions.");

            return sb.ToString();
        }

        public async Task<bool> ValidatePhoneNumberAsync(string phoneNumber)
        {
            // Basic phone number validation
            var cleaned = phoneNumber.Replace(" ", "").Replace("-", "").Replace("(", "").Replace(")", "");
            return cleaned.Length >= 10 && cleaned.All(char.IsDigit);
        }

        public async Task<bool> SendBulkInvoiceAsync(List<int> invoiceIds, List<string> phoneNumbers)
        {
            var results = new List<bool>();
            
            for (int i = 0; i < invoiceIds.Count && i < phoneNumbers.Count; i++)
            {
                var result = await SendInvoiceViaWhatsAppAsync(invoiceIds[i], phoneNumbers[i]);
                results.Add(result);
            }
            
            return results.All(r => r);
        }

        private string CreateWhatsAppUrl(string phoneNumber, string message)
        {
            var encodedMessage = Uri.EscapeDataString(message);
            return $"https://wa.me/{phoneNumber}?text={encodedMessage}";
        }

        private string FormatPhoneNumber(string phoneNumber)
        {
            // Remove all non-digit characters
            var cleaned = new string(phoneNumber.Where(char.IsDigit).ToArray());
            
            // Add country code if not present (assuming India +91)
            if (cleaned.Length == 10)
            {
                cleaned = "91" + cleaned;
            }
            
            return cleaned;
        }

        private async Task LogNotificationAsync(int invoiceId, string recipient, string message, NotificationStatus status)
        {
            try
            {
                var notification = new Notification
                {
                    Type = NotificationType.WhatsApp,
                    Recipient = recipient,
                    Subject = $"Invoice {invoiceId}",
                    Message = message,
                    Status = status,
                    InvoiceId = invoiceId,
                    CreatedDate = DateTime.Now
                };

                _context.Notifications.Add(notification);
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error logging notification: {ex.Message}");
            }
        }
    }
} 