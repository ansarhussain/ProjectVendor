using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace VendorProject.EF.Data
{
    /// <summary>
    /// Factory for creating DbContext instances at design time (for migrations).
    /// This allows EF Core tools to create the DbContext without a running application.
    /// Reads connection string from appsettings.json in the VendorProject folder.
    /// </summary>
    public class DesignTimeDbContextFactory : IDesignTimeDbContextFactory<MarketplaceDbContext>
    {
        public MarketplaceDbContext CreateDbContext(string[] args)
        {
            var configuration = new ConfigurationBuilder()
                .SetBasePath(Path.Combine(Directory.GetCurrentDirectory(), "..", "VendorProject"))
                .AddJsonFile("appsettings.json")
                .Build();

            var connectionString = configuration.GetConnectionString("DefaultConnection");

            var optionsBuilder = new DbContextOptionsBuilder<MarketplaceDbContext>();
            optionsBuilder.UseSqlServer(connectionString);

            return new MarketplaceDbContext(optionsBuilder.Options);
        }
    }
}
