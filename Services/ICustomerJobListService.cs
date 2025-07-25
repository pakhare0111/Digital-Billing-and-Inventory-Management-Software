using Billing_Software.Models;

namespace Billing_Software.Services
{
    public interface ICustomerJobListService
    {
        Task<IEnumerable<CustomerJobList>> GetCustomerJobListsAsync(int customerId);
        Task<IEnumerable<CustomerJobList>> GetSelectedCustomerJobListsAsync(int customerId);
        Task<IEnumerable<CustomerJobList>> GetCompletedCustomerJobListsAsync(int customerId);
        Task<IEnumerable<CustomerJobList>> GetAvailableJobsForNewVisitAsync(int customerId);
        Task<IEnumerable<CustomerJobList>> GetInvoicedJobsAsync(int customerId);
        Task<CustomerJobList?> GetCustomerJobListByCustomerAndJobAsync(int customerId, int jobListId);
        Task<bool> CustomerJobListExistsAsync(int customerId, int jobListId);
        Task InitializeCustomerJobListsAsync(int customerId);
        Task<bool> ToggleJobSelectionAsync(int customerId, int jobListId);
        Task<bool> MarkJobAsCompletedAsync(int customerId, int jobListId);
        Task<bool> MarkJobAsIncompleteAsync(int customerId, int jobListId);
        Task<CustomerJobList> UpdateCustomerJobListAsync(CustomerJobList customerJobList);
        Task MarkJobsAsInvoicedAsync(int customerId, int invoiceId, IEnumerable<int> jobIds);
        Task ResetJobsForNewVisitAsync(int customerId);
        Task<IEnumerable<CustomerJobList>> GetJobHistoryAsync(int customerId);
    }
} 