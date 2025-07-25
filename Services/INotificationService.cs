using Billing_Software.Models;

namespace Billing_Software.Services
{
    public interface INotificationService
    {
        Task<IEnumerable<Notification>> GetAllNotificationsAsync();
        Task<Notification?> GetNotificationByIdAsync(int id);
        Task<Notification> CreateNotificationAsync(Notification notification);
        Task<Notification> UpdateNotificationAsync(Notification notification);
        Task<bool> DeleteNotificationAsync(int id);
        Task<IEnumerable<Notification>> GetNotificationsByTypeAsync(NotificationType type);
        Task<IEnumerable<Notification>> GetNotificationsByStatusAsync(NotificationStatus status);
        Task<IEnumerable<Notification>> GetPendingNotificationsAsync();
        Task<bool> SendNotificationAsync(int notificationId);
        Task<bool> SendBulkNotificationsAsync(IEnumerable<Notification> notifications);
        Task<IEnumerable<Notification>> GetNotificationsByInvoiceAsync(int invoiceId);
        Task<IEnumerable<Notification>> GetNotificationsByServiceRecordAsync(int serviceRecordId);
        Task<IEnumerable<Notification>> GetNotificationsByCustomerAsync(int customerId);
    }
} 