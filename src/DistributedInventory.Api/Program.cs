using DistributedInventory.Core.Dtos;
using DistributedInventory.Core.Exceptions;
using DistributedInventory.Core.Models.Entities;
using DistributedInventory.Core.Services.Inventory;
using DistributedInventory.Infrastructure.Repository;
using DistributedInventory.Infrastructure.Services.EfInventoryService;
using Microsoft.EntityFrameworkCore;

namespace DistributedInventory.Api
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services
            builder.Services.AddAuthorization();
            var path = Path.GetFullPath("..//../data");

            var sqLiteConnectionString = builder.Configuration.GetConnectionString("Default") 
                                         ?? $"Data Source={path}/inventory.db;Mode=ReadWriteCreate;Pooling=True";
            
            builder.Services.AddDbContext<AppDbContext>(opt => opt.UseSqlite(sqLiteConnectionString));
            
            builder.Services.AddScoped<IInventoryService, EfInventoryService>();

            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();

            var app = builder.Build();

            Directory.CreateDirectory(path);
            
            //Mock db
            using (var scope = app.Services.CreateScope())
            {
                var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
                db.Database.EnsureCreated();

                if (!db.InventoryItems.Any())
                {
                    db.InventoryItems.AddRange(
                        new InventoryItem { Sku = "SKU-001", StoreId = "S1", OnHand = 10, Reserved = 0, Version = 1 },
                        new InventoryItem { Sku = "SKU-002", StoreId = "S1", OnHand = 20, Reserved = 0, Version = 1 },
                        new InventoryItem { Sku = "SKU-003", StoreId = "S2", OnHand = 15, Reserved = 0, Version = 1 }
                    );
                    db.SaveChanges();
                }
            }
            
            // Configure the HTTP request pipeline
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.UseHttpsRedirection();

            app.UseAuthorization();

            //Endpoints
            // Consulta item por loja
            app.MapGet("/v1/stores/{storeId}/stock/{sku}", async (IInventoryService service, string storeId, string sku, CancellationToken cancellationToken) =>
                {
                    InventoryItem? item = await service.GetAsync(sku, storeId, cancellationToken);
                    if (item == null) return Results.NotFound();
                    return Results.Ok(item);
                })
                .WithName("get item")
                .WithOpenApi();

            // Cria uma reserva
            app.MapPost("/v1/stock/reserve", async (IInventoryService service, ReserveRequest request, CancellationToken cancellationToken) =>
                {
                    try
                    {
                        Reserve reserve = await service.ReserveAsync(request, cancellationToken);
                        return Results.Created($"/v1/stock/reserve/{reserve.Id}", reserve);
                    }
                    catch (ConcurrencyException)
                    {
                        return Results.StatusCode(409);
                    }
                    catch (OutOfStockException)
                    {
                        return Results.BadRequest(new { code = "OUT_OF_STOCK" });
                    }
                })
                .WithName("create reserve")
                .WithOpenApi();
            
            // Confirma a reserva
            app.MapPost("/v1/stock/confirm/{reservationId}",
                async (IInventoryService svc, Guid reservationId) =>
                {
                    try
                    {
                        await svc.ConfirmAsync(reservationId);
                        return Results.Ok();
                    }
                    catch (NotFoundException)    { return Results.NotFound(); }
                    catch (OutOfStockException)  { return Results.Conflict(new { code = "OUT_OF_STOCK" }); }
                    catch (ConcurrencyException) { return Results.StatusCode(409); }
                })
                .WithName("confirm reserve")
                .WithOpenApi();

            // Cancela a reserva
            app.MapPost("/v1/stock/cancel/{reservationId}",
                async (IInventoryService service, Guid reservationId) =>
                {
                    try
                    {
                        await service.CancelAsync(reservationId);
                        return Results.Ok();
                    }
                    catch (NotFoundException)    { return Results.NotFound(); }
                    catch (ConcurrencyException) { return Results.StatusCode(409); }
                })
                .WithName("cancel reserve")
                .WithOpenApi();

            // Reposição de estoque 
            app.MapPost("/v1/stock/restock",
                async (IInventoryService service, RestockRequest req) =>
                {
                    try
                    {
                        await service.RestockAsync(req.Sku, req.StoreId, req.Qty, req.ExpectedVersion);
                        return Results.Ok();
                    }
                    catch (NotFoundException)    { return Results.NotFound(); }
                    catch (ConcurrencyException) { return Results.StatusCode(412); }
                })
                .WithName("restock")
                .WithOpenApi();
            
            app.Run();
        }
    }
}