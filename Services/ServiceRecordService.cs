using Billing_Software.Data;
using Billing_Software.Models;
using Microsoft.EntityFrameworkCore;

namespace Billing_Software.Services
{
    public class ServiceRecordService : IServiceRecordService
    {
        private readonly ApplicationDbContext _context;

        public ServiceRecordService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<ServiceRecord>> GetAllServiceRecordsAsync()
        {
            return await _context.ServiceRecords
                .Include(s => s.Customer)
                .Include(s => s.Vehicle)
                .OrderByDescending(s => s.CreatedDate)
                .ToListAsync();
        }

        public async Task<ServiceRecord?> GetServiceRecordByIdAsync(int id)
        {
            return await _context.ServiceRecords
                .Include(s => s.Customer)
                .Include(s => s.Vehicle)
                .FirstOrDefaultAsync(s => s.Id == id);
        }

        public async Task<ServiceRecord> CreateServiceRecordAsync(ServiceRecord serviceRecord)
        {
            serviceRecord.CreatedDate = DateTime.Now;
            serviceRecord.LastModified = DateTime.Now;
            _context.ServiceRecords.Add(serviceRecord);
            await _context.SaveChangesAsync();
            return serviceRecord;
        }

        public async Task<ServiceRecord> UpdateServiceRecordAsync(ServiceRecord serviceRecord)
        {
            var existingService = await _context.ServiceRecords.FindAsync(serviceRecord.Id);
            if (existingService == null)
                throw new ArgumentException("Service record not found");

            existingService.CustomerId = serviceRecord.CustomerId;
            existingService.VehicleId = serviceRecord.VehicleId;
            existingService.ServiceType = serviceRecord.ServiceType;
            existingService.Description = serviceRecord.Description;
            existingService.ServiceDate = serviceRecord.ServiceDate;
            existingService.NextServiceDate = serviceRecord.NextServiceDate;
            existingService.Cost = serviceRecord.Cost;
            existingService.Status = serviceRecord.Status;
            existingService.Notes = serviceRecord.Notes;
            existingService.Technician = serviceRecord.Technician;
            existingService.InvoiceId = serviceRecord.InvoiceId;
            existingService.LastModified = DateTime.Now;

            await _context.SaveChangesAsync();
            return existingService;
        }

        public async Task<bool> DeleteServiceRecordAsync(int id)
        {
            var serviceRecord = await _context.ServiceRecords.FindAsync(id);
            if (serviceRecord == null)
                return false;

            _context.ServiceRecords.Remove(serviceRecord);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<IEnumerable<ServiceRecord>> GetServiceRecordsByCustomerAsync(int customerId)
        {
            return await _context.ServiceRecords
                .Include(s => s.Customer)
                .Include(s => s.Vehicle)
                .Where(s => s.CustomerId == customerId)
                .OrderByDescending(s => s.CreatedDate)
                .ToListAsync();
        }

        public async Task<IEnumerable<ServiceRecord>> GetServiceRecordsByStatusAsync(ServiceStatus status)
        {
            return await _context.ServiceRecords
                .Include(s => s.Customer)
                .Include(s => s.Vehicle)
                .Where(s => s.Status == status)
                .OrderByDescending(s => s.CreatedDate)
                .ToListAsync();
        }

        public async Task<IEnumerable<ServiceRecord>> GetUpcomingServicesAsync(DateTime fromDate, DateTime toDate)
        {
            return await _context.ServiceRecords
                .Include(s => s.Customer)
                .Include(s => s.Vehicle)
                .Where(s => s.ServiceDate >= fromDate && s.ServiceDate <= toDate && s.Status == ServiceStatus.Scheduled)
                .OrderBy(s => s.ServiceDate)
                .ToListAsync();
        }

        public async Task<IEnumerable<ServiceRecord>> GetOverdueServicesAsync()
        {
            return await _context.ServiceRecords
                .Include(s => s.Customer)
                .Include(s => s.Vehicle)
                .Where(s => s.ServiceDate < DateTime.Today && s.Status == ServiceStatus.Scheduled)
                .OrderBy(s => s.ServiceDate)
                .ToListAsync();
        }

        public async Task<bool> SendServiceReminderAsync(int serviceRecordId)
        {
            // Implementation for sending service reminders
            await Task.Delay(100); // Simulate sending reminder
            return true;
        }

        public async Task<IEnumerable<ServiceRecord>> GetServiceRecordsByVehicleAsync(int vehicleId)
        {
            return await _context.ServiceRecords
                .Include(s => s.Customer)
                .Include(s => s.Vehicle)
                .Where(s => s.VehicleId == vehicleId)
                .OrderByDescending(s => s.CreatedDate)
                .ToListAsync();
        }

        public async Task<ServiceRecord?> GetLastServiceRecordAsync(int customerId, int? vehicleId = null)
        {
            var query = _context.ServiceRecords
                .Include(s => s.Customer)
                .Include(s => s.Vehicle)
                .Where(s => s.CustomerId == customerId);

            if (vehicleId.HasValue)
                query = query.Where(s => s.VehicleId == vehicleId);

            return await query
                .OrderByDescending(s => s.ServiceDate)
                .FirstOrDefaultAsync();
        }
    }
} 