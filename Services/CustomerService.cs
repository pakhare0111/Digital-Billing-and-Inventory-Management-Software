using Billing_Software.Data;
using Billing_Software.Models;
using Microsoft.EntityFrameworkCore;

namespace Billing_Software.Services
{
    public class CustomerService : ICustomerService
    {
        private readonly ApplicationDbContext _context;

        public CustomerService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Customer>> GetAllCustomersAsync()
        {
            return await _context.Customers
                .Include(c => c.Vehicles)
                .Include(c => c.Invoices)
                .Include(c => c.ServiceRecords)
                .OrderByDescending(c => c.CreatedDate)
                .ToListAsync();
        }

        public async Task<Customer?> GetCustomerByIdAsync(int id)
        {
            return await _context.Customers
                .Include(c => c.Vehicles)
                .Include(c => c.Invoices)
                .Include(c => c.ServiceRecords)
                .FirstOrDefaultAsync(c => c.Id == id);
        }

        public async Task<Customer?> GetCustomerByPhoneAsync(string phone)
        {
            return await _context.Customers
                .Include(c => c.Vehicles)
                .FirstOrDefaultAsync(c => c.Phone == phone);
        }

        public async Task<Customer> CreateCustomerAsync(Customer customer)
        {
            customer.CreatedDate = DateTime.Now;
            _context.Customers.Add(customer);
            await _context.SaveChangesAsync();
            return customer;
        }

        public async Task<Customer> UpdateCustomerAsync(Customer customer)
        {
            var existingCustomer = await _context.Customers.FindAsync(customer.Id);
            if (existingCustomer == null)
                throw new ArgumentException("Customer not found");

            existingCustomer.Name = customer.Name;
            existingCustomer.Phone = customer.Phone;
            existingCustomer.Email = customer.Email;
            existingCustomer.Address = customer.Address;
            existingCustomer.City = customer.City;
            existingCustomer.State = customer.State;
            existingCustomer.ZipCode = customer.ZipCode;
            existingCustomer.IsActive = customer.IsActive;

            await _context.SaveChangesAsync();
            return existingCustomer;
        }

        public async Task<bool> DeleteCustomerAsync(int id)
        {
            var customer = await _context.Customers
                .Include(c => c.Vehicles)
                .Include(c => c.Invoices)
                .Include(c => c.ServiceRecords)
                .FirstOrDefaultAsync(c => c.Id == id);
                
            if (customer == null)
                return false;

            // Remove all related data first
            _context.Vehicles.RemoveRange(customer.Vehicles);
            _context.Invoices.RemoveRange(customer.Invoices);
            _context.ServiceRecords.RemoveRange(customer.ServiceRecords);
            
            // Remove the customer
            _context.Customers.Remove(customer);
            
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<IEnumerable<Customer>> SearchCustomersAsync(string searchTerm)
        {
            if (string.IsNullOrWhiteSpace(searchTerm))
                return await GetAllCustomersAsync();

            return await _context.Customers
                .Include(c => c.Vehicles)
                .Where(c => c.IsActive && 
                           (c.Name.Contains(searchTerm) || 
                            c.Phone.Contains(searchTerm) || 
                            (c.Email != null && c.Email.Contains(searchTerm))))
                .OrderByDescending(c => c.CreatedDate)
                .ToListAsync();
        }

        public async Task<IEnumerable<Vehicle>> GetCustomerVehiclesAsync(int customerId)
        {
            return await _context.Vehicles
                .Where(v => v.CustomerId == customerId)
                .OrderByDescending(v => v.CreatedDate)
                .ToListAsync();
        }

        public async Task<IEnumerable<Invoice>> GetCustomerInvoicesAsync(int customerId)
        {
            return await _context.Invoices
                .Where(i => i.CustomerId == customerId)
                .OrderByDescending(i => i.CreatedDate)
                .ToListAsync();
        }

        public async Task<IEnumerable<ServiceRecord>> GetCustomerServiceRecordsAsync(int customerId)
        {
            return await _context.ServiceRecords
                .Include(s => s.Vehicle)
                .Where(s => s.CustomerId == customerId)
                .OrderByDescending(s => s.CreatedDate)
                .ToListAsync();
        }

        public async Task<bool> CustomerExistsAsync(string phone)
        {
            return await _context.Customers.AnyAsync(c => c.Phone == phone && c.IsActive);
        }

        public async Task<bool> CanDeleteCustomerAsync(int id)
        {
            var customer = await _context.Customers
                .Include(c => c.Vehicles)
                .Include(c => c.Invoices)
                .Include(c => c.ServiceRecords)
                .FirstOrDefaultAsync(c => c.Id == id);
                
            return customer != null;
        }
    }
} 