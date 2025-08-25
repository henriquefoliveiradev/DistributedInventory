namespace DistributedInventory.Core.Dtos
{
    public abstract record RestockRequest(string Sku, string StoreId, int Qty, long? ExpectedVersion);
}
