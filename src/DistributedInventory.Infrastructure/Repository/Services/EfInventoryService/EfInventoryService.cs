using System.Text.Json;
using DistributedInventory.Core.Dtos;
using DistributedInventory.Core.Enums;
using DistributedInventory.Core.Exceptions;
using DistributedInventory.Core.Models;
using DistributedInventory.Core.Models.Entities;
using DistributedInventory.Core.Services.Inventory;
using Microsoft.EntityFrameworkCore;

namespace DistributedInventory.Infrastructure.Repository.Services.EfInventoryService
{
    public class EfInventoryService : IInventoryService
    {
        private readonly AppDbContext _db;
        
        public EfInventoryService(AppDbContext db)
        {
            _db = db;
        }

        public async Task<InventoryItem?> GetAsync(string sku, string storeId, CancellationToken ct = default)
            => await _db.InventoryItems.FindAsync([sku, storeId], ct);

        public async Task<Reserve> ReserveAsync(ReserveRequest request, CancellationToken cancellationToken = default)
        {
            await using var transaction = await _db.Database.BeginTransactionAsync(cancellationToken);

            var item = await _db.InventoryItems
                .FirstOrDefaultAsync(i => i.Sku == request.Sku && i.StoreId == request.StoreId, cancellationToken);

            if (item is null) throw new OutOfStockException();

            var available = item.OnHand - item.Reserved;
            if (available < request.Qty) throw new OutOfStockException();

            // controle otimista simples por versão
            item.Reserved += request.Qty;
            item.Version++;

            var res = new Reserve
            {
                Id = Guid.NewGuid(),
                Sku = request.Sku,
                StoreId = request.StoreId,
                Qty = request.Qty,
                Status = ReserveStatus.Pending,
                ExpiresAt = DateTime.UtcNow.AddSeconds(request.TtlSeconds)
            };
            _db.Reservations.Add(res);

            _db.Outbox.Add(new OutboxEvent
            {
                Type = "ReservationCreated",
                Payload = JsonSerializer.Serialize(new { res.Id, res.Sku, res.StoreId, res.Qty })
            });

            await _db.SaveChangesAsync(cancellationToken);
            await transaction.CommitAsync(cancellationToken);

            return res;
        }
    }
}