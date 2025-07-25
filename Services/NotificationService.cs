using Billing_Software.Data;
using Billing_Software.Models;
using Microsoft.EntityFrameworkCore;

namespace Billing_Software.Services
{
    public class NotificationService : INotificationService
    {
        private readonly ApplicationDbContext _context;

        public NotificationService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Notification>> GetAllNotificationsAsync()
        {
            return await _context.Notifications
                .OrderByDescending(n => n.CreatedDate)
                .ToListAsync();
        }

        public async Task<Notification?> GetNotificationByIdAsync(int id)
        {
            return await _context.Notifications
                .FirstOrDefaultAsync(n => n.Id == id);
        }

        public async Task<Notification> CreateNotificationAsync(Notification notification)
        {
            notification.CreatedDate = DateTime.Now;
            _context.Notifications.Add(notification);
            await _context.SaveChangesAsync();
            return notification;
        }

        public async Task<Notification> UpdateNotificationAsync(Notification notification)
        {
            var existingNotification = await _context.Notifications.FindAsync(notification.Id);
            if (existingNotification == null)
                throw new ArgumentException("Notification not found");

            existingNotification.Type = notification.Type;
            existingNotification.Recipient = notification.Recipient;
            existingNotification.Subject = notification.Subject;
            existingNotification.Message = notification.Message;
            existingNotification.Status = notification.Status;
            existingNotification.SentDate = notification.SentDate;
            existingNotification.ErrorMessage = notification.ErrorMessage;

            await _context.SaveChangesAsync();
            return existingNotification;
        }

        public async Task<bool> DeleteNotificationAsync(int id)
        {
            var notification = await _context.Notifications.FindAsync(id);
            if (notification == null)
                return false;

            _context.Notifications.Remove(notification);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<IEnumerable<Notification>> GetNotificationsByTypeAsync(NotificationType type)
        {
            return await _context.Notifications
                .Where(n => n.Type == type)
                .OrderByDescending(n => n.CreatedDate)
                .ToListAsync();
        }

        public async Task<IEnumerable<Notification>> GetNotificationsByStatusAsync(NotificationStatus status)
        {
            return await _context.Notifications
                .Where(n => n.Status == status)
                .OrderByDescending(n => n.CreatedDate)
                .ToListAsync();
        }

        public async Task<IEnumerable<Notification>> GetPendingNotificationsAsync()
        {
            return await _context.Notifications
                .Where(n => n.Status == NotificationStatus.Pending)
                .OrderBy(n => n.CreatedDate)
                .ToListAsync();
        }

        public async Task<bool> SendNotificationAsync(int notificationId)
        {
            var notification = await _context.Notifications.FindAsync(notificationId);
            if (notification == null)
                return false;

            try
            {
                // Implementation for sending notification based on type
                notification.Status = NotificationStatus.Sent;
                notification.SentDate = DateTime.Now;
                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                notification.Status = NotificationStatus.Failed;
                notification.ErrorMessage = ex.Message;
                await _context.SaveChangesAsync();
                return false;
            }
        }

        public async Task<bool> SendBulkNotificationsAsync(IEnumerable<Notification> notifications)
        {
            var successCount = 0;
            var totalCount = notifications.Count();

            foreach (var notification in notifications)
            {
                if (await SendNotificationAsync(notification.Id))
                    successCount++;
            }

            return successCount == totalCount;
        }

        public async Task<IEnumerable<Notification>> GetNotificationsByInvoiceAsync(int invoiceId)
        {
            return await _context.Notifications
                .Where(n => n.InvoiceId == invoiceId)
                .OrderByDescending(n => n.CreatedDate)
                .ToListAsync();
        }

        public async Task<IEnumerable<Notification>> GetNotificationsByServiceRecordAsync(int serviceRecordId)
        {
            return await _context.Notifications
                .Where(n => n.ServiceRecordId == serviceRecordId)
                .OrderByDescending(n => n.CreatedDate)
                .ToListAsync();
        }

        public async Task<IEnumerable<Notification>> GetNotificationsByCustomerAsync(int customerId)
        {
            return await _context.Notifications
                .Where(n => n.CustomerId == customerId)
                .OrderByDescending(n => n.CreatedDate)
                .ToListAsync();
        }
    }
} 