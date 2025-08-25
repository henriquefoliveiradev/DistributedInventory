using DistributedInventory.Core.Models.Entities;
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

        protected override async Task ExecuteAsync(CancellationToken cancellationToken)
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                using IServiceScope scope = _scopeFactory.CreateScope();
                await using var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

                List<OutboxEvent> listPendings = await db.Outbox
                    .Where(record => record.ProcessedAt == null)
                    .OrderBy(record => record.OccurredAt)
                    .Take(50)
                    .ToListAsync(cancellationToken);

                foreach (OutboxEvent msg in listPendings)
                {
                    // TODO: publicar no tópico; por enquanto estou so logando no console, vou deixar o link do repo no github com a versão completa implementando 
                    //a publicação no tópico e consumo com kafka
                    Console.WriteLine($"[OUTBOX] {msg.Type} {msg.Payload}");
                    msg.ProcessedAt = DateTime.UtcNow;
                }

                if (listPendings.Count > 0)
                    await db.SaveChangesAsync(cancellationToken);

                await Task.Delay(300, cancellationToken);
            }
        }
    }
}