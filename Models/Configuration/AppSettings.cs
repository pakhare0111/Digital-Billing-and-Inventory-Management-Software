namespace Billing_Software.Models.Configuration
{
    public class AppSettings
    {
        public string CompanyName { get; set; } = string.Empty;
        public string CompanyAddress { get; set; } = string.Empty;
        public string CompanyPhone { get; set; } = string.Empty;
        public string CompanyEmail { get; set; } = string.Empty;
        public string Currency { get; set; } = "USD";
        public decimal TaxRate { get; set; } = 0.10m;
        public string LogoPath { get; set; } = string.Empty;
        public string WhatsAppMethod { get; set; } = "Manual"; // "Manual" or "Automated"
        public bool AutoSendWhatsAppOnInvoiceCreate { get; set; } = false; // Enable/disable automatic WhatsApp sending
    }
} 