namespace DistributedInventory.Core.Dtos
{
    public abstract record ReserveRequest(string Sku, string StoreId, int Qty, string ClientId, int TtlSeconds);
}