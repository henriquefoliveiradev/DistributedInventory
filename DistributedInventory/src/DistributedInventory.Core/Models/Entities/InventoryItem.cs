namespace DistributedInventory.Core.Models.Entities
{
    public class InventoryItem
    {
        public string Sku { get; set; } = default!;
        public string StoreId { get; set; } = default!;
        public int OnHand { get; set; }
        public int Reserved { get; set; }
        public long Version { get; set; }
    }
}