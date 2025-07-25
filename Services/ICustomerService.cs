using Billing_Software.Models;

namespace Billing_Software.Services
{
    public interface ICustomerService
    {
        Task<IEnumerable<Customer>> GetAllCustomersAsync();
        Task<Customer?> GetCustomerByIdAsync(int id);
        Task<Customer?> GetCustomerByPhoneAsync(string phone);
        Task<Customer> CreateCustomerAsync(Customer customer);
        Task<Customer> UpdateCustomerAsync(Customer customer);
        Task<bool> DeleteCustomerAsync(int id);
        Task<IEnumerable<Customer>> SearchCustomersAsync(string searchTerm);
        Task<IEnumerable<Vehicle>> GetCustomerVehiclesAsync(int customerId);
        Task<IEnumerable<Invoice>> GetCustomerInvoicesAsync(int customerId);
        Task<IEnumerable<ServiceRecord>> GetCustomerServiceRecordsAsync(int customerId);
        Task<bool> CustomerExistsAsync(string phone);
        Task<bool> CanDeleteCustomerAsync(int id);
    }
} 