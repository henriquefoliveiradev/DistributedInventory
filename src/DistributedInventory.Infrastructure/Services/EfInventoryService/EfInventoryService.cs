using System.Text.Json;
using DistributedInventory.Core.Dtos;
using DistributedInventory.Core.Enums;
using DistributedInventory.Core.Exceptions;
using DistributedInventory.Core.Models.Entities;
using DistributedInventory.Core.Services.Inventory;
using DistributedInventory.Infrastructure.Repository;
using Microsoft.EntityFrameworkCore;

namespace DistributedInventory.Infrastructure.Services.EfInventoryService
{
    public class EfInventoryService : IInventoryService
    {
        private readonly AppDbContext _db;
        
        public EfInventoryService(AppDbContext db)
        {
            _db = db;
        }

        public async Task<InventoryItem?> GetAsync(string sku, string storeId, CancellationToken cancellationToken = default)
            => await _db.InventoryItems.FindAsync([sku, storeId], cancellationToken);

        public async Task<Reserve> ReserveAsync(ReserveRequest request, CancellationToken cancellationToken = default)
        {
            await using var transaction = await _db.Database.BeginTransactionAsync(cancellationToken);

            InventoryItem? item = await _db.InventoryItems
                                      .FirstOrDefaultAsync(i => i.Sku == request.Sku && i.StoreId == request.StoreId, cancellationToken)
                                  ?? throw new NotFoundException("SKU/store não encontrado");

            int available = item.OnHand - item.Reserved;
            if (available < request.Qty) throw new OutOfStockException("Estoque insuficiente");

            // controle otimista simples por versão
            item.Reserved += request.Qty;
            item.Version++;

            var reserve = new Reserve
            {
                Id = Guid.NewGuid(),
                Sku = request.Sku,
                StoreId = request.StoreId,
                Qty = request.Qty,
                Status = ReserveStatus.Pending,
                ExpiresAt = DateTime.UtcNow.AddSeconds(request.TtlSeconds),
                ClientId = request.ClientId
            };
            _db.Reservations.Add(reserve);

            
            _db.Outbox.Add(new OutboxEvent
            {
                Type = "ReservationCreated",
                Payload = JsonSerializer.Serialize(new { reserve.Id, reserve.Sku, reserve.StoreId, reserve.Qty, item.Version, EventId = Guid.NewGuid() })
            });

            try
            {
                await _db.SaveChangesAsync(cancellationToken);
                await transaction.CommitAsync(cancellationToken);
                return reserve;
            }
            catch (DbUpdateConcurrencyException)
            {
                throw new ConcurrencyException("Conflito de versão em Reserve");
            }
        }

        public async Task ConfirmAsync(Guid reservationId, CancellationToken cancellationToken = default)
        {
            await using var transaction = await _db.Database.BeginTransactionAsync(cancellationToken);

            Reserve reserve = await _db.Reservations.FindAsync([reservationId], cancellationToken)
                          ?? throw new NotFoundException("Reserva não encontrada");
            if (reserve.Status != ReserveStatus.Pending) return;

            var item = await _db.InventoryItems.FindAsync([reserve.Sku, reserve.StoreId], cancellationToken)
                       ?? throw new NotFoundException("SKU/store não encontrado");
            
            if (item.OnHand < reserve.Qty) throw new OutOfStockException("OnHand insuficiente na confirmação");
            item.OnHand -= reserve.Qty;
            item.Reserved -= reserve.Qty;
            item.Version++;

            reserve.Status = ReserveStatus.Confirmed;

            _db.Outbox.Add(new OutboxEvent
            {
                Type = "StockDebited",
                Payload = JsonSerializer.Serialize(new
                {
                    reserve.Id, reserve.Sku, reserve.StoreId, reserve.Qty,
                    item.OnHand, item.Reserved, item.Version, EventId = Guid.NewGuid()
                })
            });

            try
            {
                await _db.SaveChangesAsync(cancellationToken);
                await transaction.CommitAsync(cancellationToken);
            }
            catch (DbUpdateConcurrencyException)
            {
                throw new ConcurrencyException("Conflito de versão em Confirm");
            }
        }

        public async Task CancelAsync(Guid reservationId, CancellationToken cancellationToken = default)
        {
            await using var transaction = await _db.Database.BeginTransactionAsync(cancellationToken);

            Reserve reserve = await _db.Reservations.FindAsync([reservationId], cancellationToken)
                              ?? throw new NotFoundException("Reserva não encontrada");
            if (reserve.Status != ReserveStatus.Pending) return;

            InventoryItem item = await _db.InventoryItems.FindAsync([reserve.Sku, reserve.StoreId], cancellationToken)
                                 ?? throw new NotFoundException("SKU/store não encontrado");
            
            item.Reserved -= reserve.Qty;
            item.Version++;

            reserve.Status = ReserveStatus.Cancelled;

            _db.Outbox.Add(new OutboxEvent
            {
                Type = "ReservationCanceled",
                Payload = JsonSerializer.Serialize(new
                {
                    reserve.Id, reserve.Sku, reserve.StoreId, reserve.Qty,
                    item.OnHand, item.Reserved, item.Version, EventId = Guid.NewGuid()
                })
            });

            try
            {
                await _db.SaveChangesAsync(cancellationToken);
                await transaction.CommitAsync(cancellationToken);
            }
            catch (DbUpdateConcurrencyException)
            {
                throw new ConcurrencyException("Conflito de versão em Cancel");
            }
        }

        public async Task RestockAsync(string sku, string storeId, int qty, long? expectedVersion, CancellationToken cancellationToken = default)
        {
            ArgumentOutOfRangeException.ThrowIfNegativeOrZero(qty);

            await using var transaction = await _db.Database.BeginTransactionAsync(cancellationToken);

            InventoryItem? item = await _db.InventoryItems.FindAsync([sku, storeId], cancellationToken);
            if (item is null)
            {
                item = new InventoryItem
                {
                    Sku = sku, StoreId = storeId,
                    OnHand = 0, Reserved = 0, Version = 0
                };
                _db.InventoryItems.Add(item);
            }

            if (expectedVersion.HasValue && expectedVersion != item.Version)
                throw new ConcurrencyException($"Versão esperada {expectedVersion} != atual {item.Version}");

            item.OnHand += qty;
            item.Version++;

            _db.Outbox.Add(new OutboxEvent
            {
                Type = "Restocked",
                Payload = JsonSerializer.Serialize(new
                {
                    item.Sku, item.StoreId, Delta = qty,
                    item.OnHand, item.Reserved, item.Version, EventId = Guid.NewGuid()
                })
            });

            try
            {
                await _db.SaveChangesAsync(cancellationToken);
                await transaction.CommitAsync(cancellationToken);
            }
            catch (DbUpdateConcurrencyException)
            {
                throw new ConcurrencyException("Conflito de versão em Restock");
            }
        }
    }
}