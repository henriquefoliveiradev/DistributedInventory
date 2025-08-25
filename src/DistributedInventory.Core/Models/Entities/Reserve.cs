using DistributedInventory.Core.Enums;

namespace DistributedInventory.Core.Models.Entities
{
    public class Reserve
    {
        public Guid Id { get; set; }
        public string Sku { get; set; } = default!;
        public string StoreId { get; set; } = default!;
        public int Qty { get; set; }
        public ReserveStatus Status { get; set; } = ReserveStatus.Pending;
        public DateTime ExpiresAt { get; set; }
        public string? ClientId { get; set; }
    }
}