namespace Billing_Software.Services
{
    public interface IPdfService
    {
        Task<byte[]> GenerateInvoicePdfAsync(int invoiceId);
        Task<string> GenerateInvoicePdfFileAsync(int invoiceId);
        Task<byte[]> GenerateServiceReportPdfAsync(int serviceRecordId);
        Task<byte[]> GenerateCustomerReportPdfAsync(int customerId);
        Task<byte[]> GenerateInventoryReportPdfAsync();
        Task<string> GenerateInvoiceHtmlAsync(int invoiceId);
        Task<string> GenerateServiceReportHtmlAsync(int serviceRecordId);
    }
} 