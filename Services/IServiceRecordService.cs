using Billing_Software.Models;

namespace Billing_Software.Services
{
    public interface IServiceRecordService
    {
        Task<IEnumerable<ServiceRecord>> GetAllServiceRecordsAsync();
        Task<ServiceRecord?> GetServiceRecordByIdAsync(int id);
        Task<ServiceRecord> CreateServiceRecordAsync(ServiceRecord serviceRecord);
        Task<ServiceRecord> UpdateServiceRecordAsync(ServiceRecord serviceRecord);
        Task<bool> DeleteServiceRecordAsync(int id);
        Task<IEnumerable<ServiceRecord>> GetServiceRecordsByCustomerAsync(int customerId);
        Task<IEnumerable<ServiceRecord>> GetServiceRecordsByStatusAsync(ServiceStatus status);
        Task<IEnumerable<ServiceRecord>> GetUpcomingServicesAsync(DateTime fromDate, DateTime toDate);
        Task<IEnumerable<ServiceRecord>> GetOverdueServicesAsync();
        Task<bool> SendServiceReminderAsync(int serviceRecordId);
        Task<IEnumerable<ServiceRecord>> GetServiceRecordsByVehicleAsync(int vehicleId);
        Task<ServiceRecord?> GetLastServiceRecordAsync(int customerId, int? vehicleId = null);
    }
} 