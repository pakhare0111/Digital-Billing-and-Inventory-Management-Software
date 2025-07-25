using Billing_Software.Models;

namespace Billing_Software.Services
{
    public interface IJobListService
    {
        Task<IEnumerable<JobList>> GetAllJobListsAsync();
        Task<IEnumerable<JobList>> GetJobListsByCustomerAsync(int customerId);
        Task<JobList?> GetJobListByIdAsync(int id);
        Task<JobList?> GetJobListByNameAsync(string jobName);
        Task<JobList> CreateJobListAsync(JobList jobList);
        Task<JobList> UpdateJobListAsync(JobList jobList);
        Task<bool> DeleteJobListAsync(int id);
        Task<IEnumerable<JobList>> SearchJobListsAsync(string searchTerm, int? customerId = null);
        Task<bool> JobListExistsAsync(int id);

        // Placeholder methods for compatibility
        Task<IEnumerable<JobList>> GetSelectedJobListsByCustomerAsync(int customerId);
        Task<IEnumerable<JobList>> GetUnassignedJobListsByCustomerAsync(int customerId);
        Task<IEnumerable<JobList>> GetCompletedJobListsByCustomerAsync(int customerId);
        Task<bool> ToggleJobSelectionAsync(int jobId);
        Task<bool> MarkJobAsCompletedAsync(int jobId);
        Task<bool> MarkJobAsIncompleteAsync(int jobId);
        Task<IEnumerable<JobList>> GetCompletedJobListsAsync();
        Task<IEnumerable<JobList>> GetCustomerJobListsAsync();
    }
} 