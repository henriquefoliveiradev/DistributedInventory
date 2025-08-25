using DistributedInventory.Core.Dtos;
using DistributedInventory.Core.Exceptions;
using DistributedInventory.Core.Models.Entities;
using DistributedInventory.Core.Services.Inventory;
using DistributedInventory.Infrastructure.Repository;
using DistributedInventory.Infrastructure.Repository.Services.EfInventoryService;
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
            var summaries = new[]
            {
                "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
            };
        
            app.MapGet("/weatherforecast", (HttpContext httpContext) =>
                {
                    var forecast = Enumerable.Range(1, 5).Select(index =>
                            new WeatherForecast
                            {
                                Date = DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
                                TemperatureC = Random.Shared.Next(-20, 55),
                                Summary = summaries[Random.Shared.Next(summaries.Length)]
                            })
                        .ToArray();
                    return forecast;
                })
                .WithName("GetWeatherForecast")
                .WithOpenApi();
        
            app.MapGet("/v1/stores/{storeId}/stock/{sku}", async (IInventoryService service, string storeId, string sku, CancellationToken cancellationToken) =>
                {
                    InventoryItem? item = await service.GetAsync(sku, storeId, cancellationToken);
                    if (item == null) return Results.NotFound();
                    return Results.Ok(item);
                })
                .WithName("stores")
                .WithOpenApi();

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
                .WithName("stock")
                .WithOpenApi();
            
            app.Run();
        }
    }
}