namespace DistributedInventory.Core.Dtos
{
    public record ReserveRequest(string Sku, string StoreId, int Qty, string ClientId, int TtlSeconds);
}