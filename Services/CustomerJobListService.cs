using Microsoft.EntityFrameworkCore;
using Billing_Software.Data;
using Billing_Software.Models;

namespace Billing_Software.Services
{
    public class CustomerJobListService : ICustomerJobListService
    {
        private readonly ApplicationDbContext _context;

        public CustomerJobListService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<CustomerJobList>> GetCustomerJobListsAsync(int customerId)
        {
            return await _context.CustomerJobLists
                .Include(cjl => cjl.JobList)
                .Where(cjl => cjl.CustomerId == customerId)
                .OrderByDescending(cjl => cjl.CreatedDate)
                .ToListAsync();
        }

        public async Task<IEnumerable<CustomerJobList>> GetSelectedCustomerJobListsAsync(int customerId)
        {
            return await _context.CustomerJobLists
                .Include(cjl => cjl.JobList)
                .Where(cjl => cjl.CustomerId == customerId && cjl.IsSelected)
                .OrderByDescending(cjl => cjl.CreatedDate)
                .ToListAsync();
        }

        public async Task<IEnumerable<CustomerJobList>> GetCompletedCustomerJobListsAsync(int customerId)
        {
            return await _context.CustomerJobLists
                .Include(cjl => cjl.JobList)
                .Where(cjl => cjl.CustomerId == customerId && cjl.IsCompleted)
                .OrderByDescending(cjl => cjl.CreatedDate)
                .ToListAsync();
        }

        public async Task<CustomerJobList?> GetCustomerJobListByIdAsync(int id)
        {
            return await _context.CustomerJobLists
                .Include(cjl => cjl.JobList)
                .Include(cjl => cjl.Customer)
                .FirstOrDefaultAsync(cjl => cjl.Id == id);
        }

        public async Task<CustomerJobList?> GetCustomerJobListByCustomerAndJobAsync(int customerId, int jobListId)
        {
            return await _context.CustomerJobLists
                .Include(cjl => cjl.JobList)
                .Include(cjl => cjl.Customer)
                .FirstOrDefaultAsync(cjl => cjl.CustomerId == customerId && cjl.JobListId == jobListId);
        }

        public async Task<CustomerJobList> CreateCustomerJobListAsync(CustomerJobList customerJobList)
        {
            customerJobList.CreatedDate = DateTime.Now;
            _context.CustomerJobLists.Add(customerJobList);
            await _context.SaveChangesAsync();
            return customerJobList;
        }

        public async Task<CustomerJobList> UpdateCustomerJobListAsync(CustomerJobList customerJobList)
        {
            customerJobList.LastModified = DateTime.Now;
            _context.CustomerJobLists.Update(customerJobList);
            await _context.SaveChangesAsync();
            return customerJobList;
        }

        public async Task DeleteCustomerJobListAsync(int id)
        {
            var customerJobList = await _context.CustomerJobLists.FindAsync(id);
            if (customerJobList != null)
            {
                _context.CustomerJobLists.Remove(customerJobList);
                await _context.SaveChangesAsync();
            }
        }

        public async Task<bool> ToggleJobSelectionAsync(int customerId, int jobListId)
        {
            var customerJobList = await _context.CustomerJobLists
                .FirstOrDefaultAsync(cjl => cjl.CustomerId == customerId && cjl.JobListId == jobListId);

            if (customerJobList == null)
            {
                // Create new customer job list entry
                customerJobList = new CustomerJobList
                {
                    CustomerId = customerId,
                    JobListId = jobListId,
                    IsSelected = true,
                    CreatedDate = DateTime.Now
                };
                _context.CustomerJobLists.Add(customerJobList);
            }
            else
            {
                customerJobList.IsSelected = !customerJobList.IsSelected;
                customerJobList.LastModified = DateTime.Now;
                _context.CustomerJobLists.Update(customerJobList);
            }

            await _context.SaveChangesAsync();
            return customerJobList.IsSelected;
        }

        public async Task<bool> MarkJobAsCompletedAsync(int customerId, int jobListId)
        {
            var customerJobList = await _context.CustomerJobLists
                .FirstOrDefaultAsync(cjl => cjl.CustomerId == customerId && cjl.JobListId == jobListId);

            if (customerJobList == null)
            {
                return false;
            }

            customerJobList.IsCompleted = true;
            customerJobList.CompletedDate = DateTime.Now;
            customerJobList.LastModified = DateTime.Now;
            _context.CustomerJobLists.Update(customerJobList);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> MarkJobAsIncompleteAsync(int customerId, int jobListId)
        {
            var customerJobList = await _context.CustomerJobLists
                .FirstOrDefaultAsync(cjl => cjl.CustomerId == customerId && cjl.JobListId == jobListId);

            if (customerJobList == null)
            {
                return false;
            }

            customerJobList.IsCompleted = false;
            customerJobList.CompletedDate = null;
            customerJobList.LastModified = DateTime.Now;
            _context.CustomerJobLists.Update(customerJobList);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> CustomerJobListExistsAsync(int customerId, int jobListId)
        {
            return await _context.CustomerJobLists
                .AnyAsync(cjl => cjl.CustomerId == customerId && cjl.JobListId == jobListId);
        }

        public async Task InitializeCustomerJobListsAsync(int customerId)
        {
            // Get all available jobs
            var allJobs = await _context.JobLists.ToListAsync();
            
            // Get existing customer job lists
            var existingCustomerJobLists = await _context.CustomerJobLists
                .Where(cjl => cjl.CustomerId == customerId)
                .Select(cjl => cjl.JobListId)
                .ToListAsync();

            // Create customer job list entries for jobs that don't exist yet
            foreach (var job in allJobs)
            {
                if (!existingCustomerJobLists.Contains(job.Id))
                {
                    var customerJobList = new CustomerJobList
                    {
                        CustomerId = customerId,
                        JobListId = job.Id,
                        IsSelected = false,
                        IsCompleted = false,
                        CreatedDate = DateTime.Now
                    };
                    _context.CustomerJobLists.Add(customerJobList);
                }
            }

            await _context.SaveChangesAsync();
        }

        public async Task<IEnumerable<CustomerJobList>> GetAvailableJobsForNewVisitAsync(int customerId)
        {
            return await _context.CustomerJobLists
                .Include(cjl => cjl.JobList)
                .Where(cjl => cjl.CustomerId == customerId && 
                              !cjl.IsInvoiced && 
                              !cjl.IsResetForNewVisit)
                .OrderByDescending(cjl => cjl.CreatedDate)
                .ToListAsync();
        }

        public async Task<IEnumerable<CustomerJobList>> GetInvoicedJobsAsync(int customerId)
        {
            return await _context.CustomerJobLists
                .Include(cjl => cjl.JobList)
                .Include(cjl => cjl.Invoice)
                .Where(cjl => cjl.CustomerId == customerId && cjl.IsInvoiced)
                .OrderByDescending(cjl => cjl.InvoicedDate)
                .ToListAsync();
        }

        public async Task MarkJobsAsInvoicedAsync(int customerId, int invoiceId, IEnumerable<int> jobIds)
        {
            var jobsToMark = await _context.CustomerJobLists
                .Where(cjl => cjl.CustomerId == customerId && jobIds.Contains(cjl.JobListId))
                .ToListAsync();

            foreach (var job in jobsToMark)
            {
                job.IsInvoiced = true;
                job.InvoiceId = invoiceId;
                job.InvoicedDate = DateTime.Now;
                job.LastModified = DateTime.Now;
            }

            await _context.SaveChangesAsync();
        }

        public async Task ResetJobsForNewVisitAsync(int customerId)
        {
            var existingJobs = await _context.CustomerJobLists
                .Where(cjl => cjl.CustomerId == customerId)
                .ToListAsync();

            foreach (var job in existingJobs)
            {
                job.IsResetForNewVisit = true;
                job.ResetDate = DateTime.Now;
                job.LastModified = DateTime.Now;
            }

            await _context.SaveChangesAsync();
        }

        public async Task<IEnumerable<CustomerJobList>> GetJobHistoryAsync(int customerId)
        {
            return await _context.CustomerJobLists
                .Include(cjl => cjl.JobList)
                .Include(cjl => cjl.Invoice)
                .Where(cjl => cjl.CustomerId == customerId)
                .OrderByDescending(cjl => cjl.CreatedDate)
                .ToListAsync();
        }
    }
} 