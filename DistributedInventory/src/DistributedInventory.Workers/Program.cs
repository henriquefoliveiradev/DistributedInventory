using DistributedInventory.Infrastructure.Repository;
using Microsoft.EntityFrameworkCore;

namespace DistributedInventory.Workers
{
    public class Program
    {
        public static void Main(string[] args)
        {
            Host.CreateDefaultBuilder(args)
                .ConfigureServices((context, services) =>
                {
                    var path = Path.GetFullPath("..//../data");
                    string sqLiteConnectionString = context.Configuration.GetConnectionString("Default")
                                                    ?? $"Data Source={path}/inventory.db;Mode=ReadWriteCreate;Pooling=True;";

                    services.AddDbContext<AppDbContext>(opt => opt.UseSqlite(sqLiteConnectionString));
                    services.AddHostedService<OutboxPublisherWorker>();
                })
                .Build()
                .Run();
        }
    }
}