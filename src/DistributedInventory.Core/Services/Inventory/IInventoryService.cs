using DistributedInventory.Core.Dtos;
using DistributedInventory.Core.Models.Entities;

namespace DistributedInventory.Core.Services.Inventory
{
    public interface IInventoryService
    {
        Task<InventoryItem?> GetAsync(string sku, string storeId, CancellationToken cancellationToken);
        Task<Reserve> ReserveAsync(ReserveRequest request, CancellationToken cancellationToken);
        Task ConfirmAsync(Guid reservationId, CancellationToken cancellationToken = default);
        Task CancelAsync(Guid reservationId, CancellationToken cancellationToken = default);
        Task RestockAsync(string sku, string storeId, int qty, long? expectedVersion, CancellationToken cancellationToken = default);
    }
}