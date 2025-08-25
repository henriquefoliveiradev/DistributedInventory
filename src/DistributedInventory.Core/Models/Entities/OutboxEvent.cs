namespace DistributedInventory.Core.Models.Entities
{
    public class OutboxEvent
    {
        public Guid Id { get; set; }
        public string Type { get; set; } = default!;
        public string Payload { get; set; } = default!;
        public DateTime OccurredAt { get; set; } = DateTime.UtcNow;
        public DateTime? ProcessedAt { get; set; }
    }
}