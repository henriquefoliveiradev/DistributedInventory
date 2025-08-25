using DistributedInventory.Core.Dtos;
using DistributedInventory.Core.Models.Entities;

namespace DistributedInventory.Core.Services.Inventory
{
    public interface IInventoryService
    {
        Task<Reserve> ReserveAsync(ReserveRequest request, CancellationToken cancellationToken);
        Task<InventoryItem?> GetAsync(string sku, string storeId, CancellationToken cancellationToken);
    }
}