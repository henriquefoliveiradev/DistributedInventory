using DistributedInventory.Core.Models;
using DistributedInventory.Core.Models.Entities;
using Microsoft.EntityFrameworkCore;

namespace DistributedInventory.Infrastructure.Repository
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) {}

        public DbSet<InventoryItem> InventoryItems => Set<InventoryItem>();
        public DbSet<Reserve> Reservations => Set<Reserve>();
        public DbSet<OutboxEvent> Outbox => Set<OutboxEvent>();
        
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<InventoryItem>().HasKey(i => new { i.Sku, i.StoreId });
        }
    }
}