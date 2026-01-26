// =======================================================
// EF Core 8 - SQL Server - Code First
// Target: .NET 8, Microsoft.EntityFrameworkCore.SqlServer
// Notes:
// - Uses GUID PKs
// - Uses Fluent API for keys/indexes/relationships/precision
// - Enums stored as strings (nvarchar) for readability
// =======================================================

using Microsoft.EntityFrameworkCore;
using VendorProject.EF.Models;

namespace VendorProject.EF.Data
{
    public class MarketplaceDbContext : DbContext
    {
        public MarketplaceDbContext(DbContextOptions<MarketplaceDbContext> options) : base(options) { }

        // Users
        public DbSet<User> Users => Set<User>();
        public DbSet<UserRole> UserRoles => Set<UserRole>();
        public DbSet<UserKyc> UserKycs => Set<UserKyc>();
        public DbSet<UserOtp> UserOtps => Set<UserOtp>();
        public DbSet<UserDevice> UserDevices => Set<UserDevice>();
        public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();

        // Addresses
        public DbSet<Address> Addresses => Set<Address>();
        public DbSet<UserAddress> UserAddresses => Set<UserAddress>();
        public DbSet<UserContact> UserContacts => Set<UserContact>();

        // Products & Listings
        public DbSet<Product> Products => Set<Product>();
        public DbSet<VendorListing> VendorListings => Set<VendorListing>();

        // Orders & Payments
        public DbSet<Order> Orders => Set<Order>();
        public DbSet<OrderItem> OrderItems => Set<OrderItem>();
        public DbSet<Payment> Payments => Set<Payment>();

        // Transport
        public DbSet<Vehicle> Vehicles => Set<Vehicle>();
        public DbSet<TransportRoute> TransportRoutes => Set<TransportRoute>();

        // Shipments
        public DbSet<Shipment> Shipments => Set<Shipment>();
        public DbSet<ShipmentContact> ShipmentContacts => Set<ShipmentContact>();

        // Other
        public DbSet<ContactUnlock> ContactUnlocks => Set<ContactUnlock>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Apply all entity configurations
            modelBuilder.ApplyConfiguration(new UserConfig());
            modelBuilder.ApplyConfiguration(new UserRoleConfig());
            modelBuilder.ApplyConfiguration(new UserKycConfig());
            modelBuilder.ApplyConfiguration(new UserOtpConfig());
            modelBuilder.ApplyConfiguration(new UserDeviceConfig());
            modelBuilder.ApplyConfiguration(new RefreshTokenConfig());

            modelBuilder.ApplyConfiguration(new AddressConfig());
            modelBuilder.ApplyConfiguration(new UserAddressConfig());
            modelBuilder.ApplyConfiguration(new UserContactConfig());

            modelBuilder.ApplyConfiguration(new ProductConfig());
            modelBuilder.ApplyConfiguration(new VendorListingConfig());

            modelBuilder.ApplyConfiguration(new OrderConfig());
            modelBuilder.ApplyConfiguration(new OrderItemConfig());
            modelBuilder.ApplyConfiguration(new PaymentConfig());

            modelBuilder.ApplyConfiguration(new VehicleConfig());
            modelBuilder.ApplyConfiguration(new TransportRouteConfig());

            modelBuilder.ApplyConfiguration(new ShipmentConfig());
            modelBuilder.ApplyConfiguration(new ShipmentContactConfig());

            modelBuilder.ApplyConfiguration(new ContactUnlockConfig());
        }
    }
}
