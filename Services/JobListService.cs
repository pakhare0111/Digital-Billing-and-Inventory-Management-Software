using Microsoft.EntityFrameworkCore;
using Billing_Software.Data;
using Billing_Software.Models;

namespace Billing_Software.Services
{
    public class JobListService : IJobListService
    {
        private readonly ApplicationDbContext _context;

        public JobListService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<JobList>> GetAllJobListsAsync()
        {
            return await _context.JobLists
                .OrderByDescending(j => j.CreatedDate)
                .ToListAsync();
        }

        public async Task<IEnumerable<JobList>> GetJobListsByCustomerAsync(int customerId)
        {
            // Since JobList is no longer customer-specific, return all jobs
            return await GetAllJobListsAsync();
        }

        public async Task<JobList?> GetJobListByIdAsync(int id)
        {
            return await _context.JobLists
                .FirstOrDefaultAsync(j => j.Id == id);
        }

        public async Task<JobList?> GetJobListByNameAsync(string jobName)
        {
            return await _context.JobLists
                .FirstOrDefaultAsync(j => j.JobName == jobName);
        }

        public async Task<JobList> CreateJobListAsync(JobList jobList)
        {
            jobList.CreatedDate = DateTime.Now;
            _context.JobLists.Add(jobList);
            await _context.SaveChangesAsync();
            return jobList;
        }

        public async Task<JobList> UpdateJobListAsync(JobList jobList)
        {
            jobList.LastModified = DateTime.Now;
            _context.JobLists.Update(jobList);
            await _context.SaveChangesAsync();
            return jobList;
        }

        public async Task<bool> DeleteJobListAsync(int id)
        {
            var jobList = await _context.JobLists.FindAsync(id);
            if (jobList != null)
            {
                _context.JobLists.Remove(jobList);
                await _context.SaveChangesAsync();
                return true;
            }
            return false;
        }

        public async Task<IEnumerable<JobList>> SearchJobListsAsync(string searchTerm, int? customerId = null)
        {
            var query = _context.JobLists.AsQueryable();

            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                query = query.Where(j =>
                    j.JobName.Contains(searchTerm) ||
                    (j.Description != null && j.Description.Contains(searchTerm)));
            }

            return await query.OrderByDescending(j => j.CreatedDate).ToListAsync();
        }

        public async Task<bool> JobListExistsAsync(int id)
        {
            return await _context.JobLists.AnyAsync(j => j.Id == id);
        }

        // Placeholder methods for compatibility - these are no longer needed but kept for interface compatibility
        public async Task<IEnumerable<JobList>> GetSelectedJobListsByCustomerAsync(int customerId)
        {
            return await GetAllJobListsAsync();
        }

        public async Task<IEnumerable<JobList>> GetUnassignedJobListsByCustomerAsync(int customerId)
        {
            return await GetAllJobListsAsync();
        }

        public async Task<IEnumerable<JobList>> GetCompletedJobListsByCustomerAsync(int customerId)
        {
            return await GetAllJobListsAsync();
        }

        public async Task<bool> ToggleJobSelectionAsync(int jobId)
        {
            return true; // No longer applicable
        }

        public async Task<bool> MarkJobAsCompletedAsync(int jobId)
        {
            var jobList = await _context.JobLists.FindAsync(jobId);
            if (jobList != null)
            {
                jobList.LastModified = DateTime.Now;
                await _context.SaveChangesAsync();
                return true;
            }
            return false;
        }

        public async Task<bool> MarkJobAsIncompleteAsync(int jobId)
        {
            var jobList = await _context.JobLists.FindAsync(jobId);
            if (jobList != null)
            {
                jobList.LastModified = DateTime.Now;
                await _context.SaveChangesAsync();
                return true;
            }
            return false;
        }

        public async Task<IEnumerable<JobList>> GetCompletedJobListsAsync()
        {
            return await _context.JobLists
                .Where(j => j.LastModified.HasValue)
                .OrderByDescending(j => j.LastModified)
                .ToListAsync();
        }

        public async Task<IEnumerable<JobList>> GetCustomerJobListsAsync()
        {
            return await GetAllJobListsAsync();
        }
    }
} 