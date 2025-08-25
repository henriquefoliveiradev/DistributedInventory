using DistributedInventory.Core.Models;
using DistributedInventory.Infrastructure.Repository;
using Microsoft.EntityFrameworkCore;

namespace DistributedInventory.Workers
{
    public class OutboxPublisherWorker : BackgroundService
    {
        private readonly ILogger<OutboxPublisherWorker> _logger;
        private readonly IServiceScopeFactory _scopeFactory;
        
        public OutboxPublisherWorker(
            IServiceScopeFactory scopeFactory,
            ILogger<OutboxPublisherWorker> logger)
        {
            _scopeFactory = scopeFactory;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                using IServiceScope scope = _scopeFactory.CreateScope();
                AppDbContext db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

                List<OutboxEvent> recordsPending = await db.Outbox.Where(record => record.ProcessedAt == null)
                    .OrderBy(record => record.OccurredAt).Take(25).ToListAsync(stoppingToken);

                foreach (OutboxEvent record in recordsPending)
                {
                    // aqui você publicaria no bus; estamos só logando
                    Console.WriteLine($"[OUTBOX] {record.Type} {record.Payload}");
                    record.ProcessedAt = DateTime.UtcNow;
                }

                if (recordsPending.Count > 0)
                    await db.SaveChangesAsync(stoppingToken);

                await Task.Delay(500, stoppingToken);
            }
        }
    }
}