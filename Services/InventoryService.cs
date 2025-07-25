using Billing_Software.Data;
using Billing_Software.Models;
using Microsoft.EntityFrameworkCore;

namespace Billing_Software.Services
{
    public class InventoryService : IInventoryService
    {
        private readonly ApplicationDbContext _context;

        public InventoryService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Inventory>> GetAllInventoryAsync()
        {
            return await _context.Inventories
                .Where(i => i.IsActive)
                .OrderBy(i => i.Name)
                .ToListAsync();
        }

        public async Task<IEnumerable<Inventory>> GetAllActiveInventoryAsync()
        {
            return await _context.Inventories
                .Where(i => i.IsActive)
                .OrderBy(i => i.Name)
                .ToListAsync();
        }

        public async Task<Inventory?> GetInventoryByIdAsync(int id)
        {
            return await _context.Inventories
                .FirstOrDefaultAsync(i => i.Id == id);
        }

        public async Task<Inventory?> GetInventoryBySkuAsync(string sku)
        {
            return await _context.Inventories
                .FirstOrDefaultAsync(i => i.SKU == sku && i.IsActive);
        }

        public async Task<Inventory> CreateInventoryAsync(Inventory inventory)
        {
            inventory.CreatedDate = DateTime.Now;
            inventory.LastUpdated = DateTime.Now;
            _context.Inventories.Add(inventory);
            await _context.SaveChangesAsync();
            return inventory;
        }

        public async Task<Inventory> UpdateInventoryAsync(Inventory inventory)
        {
            var existingInventory = await _context.Inventories.FindAsync(inventory.Id);
            if (existingInventory == null)
                throw new ArgumentException("Inventory item not found");

            existingInventory.Name = inventory.Name;
            existingInventory.Description = inventory.Description;
            existingInventory.Category = inventory.Category;
            existingInventory.Price = inventory.Price;
            existingInventory.Quantity = inventory.Quantity;
            existingInventory.MinStockLevel = inventory.MinStockLevel;
            existingInventory.Unit = inventory.Unit;
            existingInventory.Supplier = inventory.Supplier;
            existingInventory.SKU = inventory.SKU;
            existingInventory.LastUpdated = DateTime.Now;

            await _context.SaveChangesAsync();
            return existingInventory;
        }

        public async Task<bool> DeleteInventoryAsync(int id)
        {
            var inventory = await _context.Inventories.FindAsync(id);
            if (inventory == null)
                return false;

            inventory.IsActive = false;
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<IEnumerable<Inventory>> SearchInventoryAsync(string searchTerm)
        {
            if (string.IsNullOrWhiteSpace(searchTerm))
                return await GetAllInventoryAsync();

            return await _context.Inventories
                .Where(i => i.IsActive && 
                           (i.Name.Contains(searchTerm) || 
                            (i.Description != null && i.Description.Contains(searchTerm)) || 
                            i.Category.Contains(searchTerm) ||
                            (i.SKU != null && i.SKU.Contains(searchTerm))))
                .OrderBy(i => i.Name)
                .ToListAsync();
        }

        public async Task<IEnumerable<Inventory>> GetLowStockItemsAsync()
        {
            return await _context.Inventories
                .Where(i => i.IsActive && i.Quantity <= i.MinStockLevel)
                .OrderBy(i => i.Quantity)
                .ToListAsync();
        }

        public async Task<IEnumerable<Inventory>> GetInventoryByCategoryAsync(string category)
        {
            return await _context.Inventories
                .Where(i => i.IsActive && i.Category == category)
                .OrderBy(i => i.Name)
                .ToListAsync();
        }

        public async Task<bool> UpdateStockLevelAsync(int inventoryId, int quantity)
        {
            var inventory = await _context.Inventories.FindAsync(inventoryId);
            if (inventory == null)
                return false;

            inventory.Quantity = quantity;
            inventory.LastUpdated = DateTime.Now;
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> UpdateStockAsync(int inventoryId, int quantity)
        {
            var inventory = await _context.Inventories.FindAsync(inventoryId);
            if (inventory == null)
                return false;

            inventory.Quantity = quantity;
            inventory.LastUpdated = DateTime.Now;
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> CheckStockAvailabilityAsync(int inventoryId, int requiredQuantity)
        {
            var inventory = await _context.Inventories.FindAsync(inventoryId);
            return inventory != null && inventory.Quantity >= requiredQuantity;
        }

        public async Task<IEnumerable<Inventory>> GetOutOfStockItemsAsync()
        {
            return await _context.Inventories
                .Where(i => i.IsActive && i.Quantity == 0)
                .OrderBy(i => i.Name)
                .ToListAsync();
        }

        public async Task<decimal> GetInventoryValueAsync()
        {
            return await _context.Inventories
                .Where(i => i.IsActive)
                .SumAsync(i => i.Price * i.Quantity);
        }

        public async Task<object> GetStockValueAsync()
        {
            var totalItems = await _context.Inventories.Where(i => i.IsActive).CountAsync();
            var totalValue = await GetInventoryValueAsync();
            var lowStockItems = await GetLowStockItemsAsync();
            var outOfStockItems = await GetOutOfStockItemsAsync();

            return new
            {
                TotalItems = totalItems,
                TotalValue = totalValue,
                LowStockCount = lowStockItems.Count(),
                OutOfStockCount = outOfStockItems.Count(),
                LowStockItems = lowStockItems,
                OutOfStockItems = outOfStockItems
            };
        }
    }
} 