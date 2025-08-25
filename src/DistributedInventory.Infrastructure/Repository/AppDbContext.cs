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
            //InventoryItem
            modelBuilder.Entity<InventoryItem>().HasKey(i => new { i.Sku, i.StoreId });
            modelBuilder.Entity<InventoryItem>().Property(i => i.Version).IsConcurrencyToken(); // controle otimista via EF

            //Reserve
            modelBuilder.Entity<Reserve>().HasKey(r => r.Id);
            modelBuilder.Entity<Reserve>().HasIndex(r => new { r.Sku, r.StoreId });

            //OutboxEvent
            modelBuilder.Entity<OutboxEvent>().HasKey(o => o.Id);
        }
    }
}