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
                    var dataDir = Environment.GetEnvironmentVariable("DATA_DIR");
                    string dbPath;

                    if (!string.IsNullOrWhiteSpace(dataDir))
                    {
                        Directory.CreateDirectory(dataDir);
                        dbPath = Path.Combine(dataDir, "inventory.db");
                    }
                    else
                    {
                        dbPath = Path.GetFullPath("..//../data/inventory.db");
                    }

                    var connectionStringSqlite = $"Data Source={dbPath};Mode=ReadWriteCreate;Pooling=True;Cache=Shared;";
                    services.AddDbContext<AppDbContext>(opt => opt.UseSqlite(connectionStringSqlite));
                    
                    services.AddHostedService<OutboxPublisherWorker>();
                })
                .Build()
                .Run();
        }
    }
}