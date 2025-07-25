using Microsoft.EntityFrameworkCore;
using Billing_Software.Models;

namespace Billing_Software.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<Customer> Customers { get; set; }
        public DbSet<Vehicle> Vehicles { get; set; }
        public DbSet<Inventory> Inventories { get; set; }
        public DbSet<Invoice> Invoices { get; set; }
        public DbSet<InvoiceItem> InvoiceItems { get; set; }
        public DbSet<ServiceRecord> ServiceRecords { get; set; }
        public DbSet<JobList> JobLists { get; set; }
        public DbSet<CustomerJobList> CustomerJobLists { get; set; }
        public DbSet<Notification> Notifications { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configure decimal precision
            modelBuilder.Entity<Inventory>()
                .Property(i => i.Price)
                .HasPrecision(18, 2);

            modelBuilder.Entity<Invoice>()
                .Property(i => i.Subtotal)
                .HasPrecision(18, 2);

            modelBuilder.Entity<Invoice>()
                .Property(i => i.TaxAmount)
                .HasPrecision(18, 2);

            modelBuilder.Entity<Invoice>()
                .Property(i => i.Total)
                .HasPrecision(18, 2);

            modelBuilder.Entity<Invoice>()
                .Property(i => i.AmountPaid)
                .HasPrecision(18, 2);

            modelBuilder.Entity<Invoice>()
                .Property(i => i.Balance)
                .HasPrecision(18, 2);

            modelBuilder.Entity<InvoiceItem>()
                .Property(item => item.UnitPrice)
                .HasPrecision(18, 2);

            modelBuilder.Entity<InvoiceItem>()
                .Property(item => item.Total)
                .HasPrecision(18, 2);

            modelBuilder.Entity<ServiceRecord>()
                .Property(s => s.Cost)
                .HasPrecision(18, 2);

            modelBuilder.Entity<JobList>()
                .Property(j => j.Price)
                .HasPrecision(18, 2);

            // Configure relationships
            modelBuilder.Entity<Customer>()
                .HasMany(c => c.Vehicles)
                .WithOne(v => v.Customer)
                .HasForeignKey(v => v.CustomerId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Customer>()
                .HasMany(c => c.Invoices)
                .WithOne(i => i.Customer)
                .HasForeignKey(i => i.CustomerId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Customer>()
                .HasMany(c => c.ServiceRecords)
                .WithOne(s => s.Customer)
                .HasForeignKey(s => s.CustomerId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Vehicle>()
                .HasMany(v => v.ServiceRecords)
                .WithOne(s => s.Vehicle)
                .HasForeignKey(s => s.VehicleId)
                .OnDelete(DeleteBehavior.SetNull);

            modelBuilder.Entity<Invoice>()
                .HasMany(i => i.InvoiceItems)
                .WithOne(item => item.Invoice)
                .HasForeignKey(item => item.InvoiceId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Inventory>()
                .HasMany(inv => inv.InvoiceItems)
                .WithOne(item => item.Inventory)
                .HasForeignKey(item => item.InventoryId)
                .OnDelete(DeleteBehavior.SetNull);

            modelBuilder.Entity<ServiceRecord>()
                .HasOne(s => s.Invoice)
                .WithMany()
                .HasForeignKey(s => s.InvoiceId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<ServiceRecord>()
                .HasOne(s => s.Vehicle)
                .WithMany(v => v.ServiceRecords)
                .HasForeignKey(s => s.VehicleId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<CustomerJobList>()
                .HasOne(cjl => cjl.Customer)
                .WithMany(c => c.CustomerJobLists)
                .HasForeignKey(cjl => cjl.CustomerId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<CustomerJobList>()
                .HasOne(cjl => cjl.JobList)
                .WithMany()
                .HasForeignKey(cjl => cjl.JobListId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<CustomerJobList>()
                .HasOne(cjl => cjl.Invoice)
                .WithMany()
                .HasForeignKey(cjl => cjl.InvoiceId)
                .OnDelete(DeleteBehavior.SetNull);

            // Configure indexes
            modelBuilder.Entity<Customer>()
                .HasIndex(c => c.Phone)
                .IsUnique();

            modelBuilder.Entity<Customer>()
                .HasIndex(c => c.Email);

            modelBuilder.Entity<Invoice>()
                .HasIndex(i => i.InvoiceNumber)
                .IsUnique();

            modelBuilder.Entity<Inventory>()
                .HasIndex(i => i.SKU)
                .IsUnique();

            modelBuilder.Entity<Vehicle>()
                .HasIndex(v => v.PlateNumber);

            // Configure default values
            modelBuilder.Entity<Invoice>()
                .Property(i => i.Status)
                .HasDefaultValue(InvoiceStatus.Draft);

            modelBuilder.Entity<ServiceRecord>()
                .Property(s => s.Status)
                .HasDefaultValue(ServiceStatus.Scheduled);

            modelBuilder.Entity<Notification>()
                .Property(n => n.Status)
                .HasDefaultValue(NotificationStatus.Pending);
        }
    }
} 