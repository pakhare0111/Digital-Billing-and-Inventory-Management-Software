namespace Billing_Software.Models.Configuration
{
    public class WhatsAppSettings
    {
        public string ApiKey { get; set; } = string.Empty;
        public string PhoneNumber { get; set; } = string.Empty;
        public string TemplateId { get; set; } = string.Empty;
        public string BaseUrl { get; set; } = "https://graph.facebook.com/v17.0";
    }
} 