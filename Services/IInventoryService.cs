using Billing_Software.Models;

namespace Billing_Software.Services
{
    public interface IInventoryService
    {
        Task<IEnumerable<Inventory>> GetAllInventoryAsync();
        Task<IEnumerable<Inventory>> GetAllActiveInventoryAsync();
        Task<Inventory?> GetInventoryByIdAsync(int id);
        Task<Inventory?> GetInventoryBySkuAsync(string sku);
        Task<Inventory> CreateInventoryAsync(Inventory inventory);
        Task<Inventory> UpdateInventoryAsync(Inventory inventory);
        Task<bool> DeleteInventoryAsync(int id);
        Task<IEnumerable<Inventory>> SearchInventoryAsync(string searchTerm);
        Task<IEnumerable<Inventory>> GetLowStockItemsAsync();
        Task<IEnumerable<Inventory>> GetInventoryByCategoryAsync(string category);
        Task<bool> UpdateStockLevelAsync(int inventoryId, int quantity);
        Task<bool> UpdateStockAsync(int inventoryId, int quantity);
        Task<bool> CheckStockAvailabilityAsync(int inventoryId, int requiredQuantity);
        Task<IEnumerable<Inventory>> GetOutOfStockItemsAsync();
        Task<decimal> GetInventoryValueAsync();
        Task<object> GetStockValueAsync();
    }
} 