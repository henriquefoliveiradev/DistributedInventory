namespace DistributedInventory.Core.Dtos
{
    public record RestockRequest(string Sku, string StoreId, int Qty, long? ExpectedVersion);
}
