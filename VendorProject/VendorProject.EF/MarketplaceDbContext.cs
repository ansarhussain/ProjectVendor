// =======================================================
// EF Core 8 - SQL Server - Code First (Complete Models)
// Target: .NET 8, Microsoft.EntityFrameworkCore.SqlServer
// Notes:
// - Uses GUID PKs
// - Uses Fluent API for keys/indexes/relationships/precision
// - Enums stored as strings (nvarchar) for readability
// =======================================================

using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using VendorProject.EF.Models;

namespace Marketplace.Data
{
    // -------------------------
    // DbContext
    // -------------------------
    public class MarketplaceDbContext : DbContext
    {
        public MarketplaceDbContext(DbContextOptions<MarketplaceDbContext> options) : base(options) { }

        public DbSet<User> Users => Set<User>();
        public DbSet<UserRole> UserRoles => Set<UserRole>();
        public DbSet<UserKyc> UserKycs => Set<UserKyc>();

        public DbSet<Address> Addresses => Set<Address>();
        public DbSet<UserAddress> UserAddresses => Set<UserAddress>();
        public DbSet<UserContact> UserContacts => Set<UserContact>();

        public DbSet<Product> Products => Set<Product>();
        public DbSet<VendorListing> VendorListings => Set<VendorListing>();

        public DbSet<Order> Orders => Set<Order>();
        public DbSet<OrderItem> OrderItems => Set<OrderItem>();
        public DbSet<Payment> Payments => Set<Payment>();

        public DbSet<Vehicle> Vehicles => Set<Vehicle>();
        public DbSet<TransportRoute> TransportRoutes => Set<TransportRoute>();

        public DbSet<Shipment> Shipments => Set<Shipment>();
        public DbSet<ShipmentContact> ShipmentContacts => Set<ShipmentContact>();

        public DbSet<ContactUnlock> ContactUnlocks => Set<ContactUnlock>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.ApplyConfiguration(new UserConfig());
            modelBuilder.ApplyConfiguration(new UserRoleConfig());
            modelBuilder.ApplyConfiguration(new UserKycConfig());

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

    // -------------------------
    // Fluent Configurations
    // -------------------------

    internal static class SqlDefaults
    {
        public const string NowUtc = "SYSUTCDATETIME()";
    }

    public class UserConfig : IEntityTypeConfiguration<User>
    {
        public void Configure(EntityTypeBuilder<User> b)
        {
            b.ToTable("users");
            b.HasKey(x => x.Id);

            b.Property(x => x.FullName).HasMaxLength(150).IsRequired();
            b.Property(x => x.Email).HasMaxLength(254);
            b.Property(x => x.Phone).HasMaxLength(20).IsRequired();
            b.Property(x => x.PasswordHash).HasMaxLength(255);
            b.Property(x => x.IsActive).HasDefaultValue(true);

            b.Property(x => x.CreatedAt).HasDefaultValueSql(SqlDefaults.NowUtc);
            b.Property(x => x.UpdatedAt).HasDefaultValueSql(SqlDefaults.NowUtc);

            b.HasIndex(x => x.Email).IsUnique().HasFilter("[Email] IS NOT NULL");
            b.HasIndex(x => x.Phone).IsUnique();

            b.HasMany(x => x.Roles).WithOne(x => x.User).HasForeignKey(x => x.UserId).OnDelete(DeleteBehavior.Cascade);
            b.HasMany(x => x.Addresses).WithOne(x => x.User).HasForeignKey(x => x.UserId).OnDelete(DeleteBehavior.Cascade);
            b.HasMany(x => x.Contacts).WithOne(x => x.User).HasForeignKey(x => x.UserId).OnDelete(DeleteBehavior.Cascade);
            b.HasMany(x => x.Kycs).WithOne(x => x.User).HasForeignKey(x => x.UserId).OnDelete(DeleteBehavior.Cascade);

            b.HasMany(x => x.VendorListings).WithOne(x => x.VendorUser).HasForeignKey(x => x.VendorUserId).OnDelete(DeleteBehavior.Restrict);

            b.HasMany(x => x.BuyerOrders).WithOne(x => x.BuyerUser).HasForeignKey(x => x.BuyerUserId).OnDelete(DeleteBehavior.Restrict);
            b.HasMany(x => x.VendorOrders).WithOne(x => x.VendorUser).HasForeignKey(x => x.VendorUserId).OnDelete(DeleteBehavior.Restrict);

            b.HasMany(x => x.Vehicles).WithOne(x => x.TransporterUser).HasForeignKey(x => x.TransporterUserId).OnDelete(DeleteBehavior.Restrict);
            b.HasMany(x => x.TransportRoutes).WithOne(x => x.TransporterUser).HasForeignKey(x => x.TransporterUserId).OnDelete(DeleteBehavior.Restrict);
            b.HasMany(x => x.ShipmentsAsTransporter).WithOne(x => x.TransporterUser).HasForeignKey(x => x.TransporterUserId).OnDelete(DeleteBehavior.Restrict);

            b.HasMany(x => x.PaymentsAsPayer).WithOne(x => x.PayerUser).HasForeignKey(x => x.PayerUserId).OnDelete(DeleteBehavior.Restrict);

            b.HasMany(x => x.ContactUnlocksAsBuyer).WithOne(x => x.BuyerUser).HasForeignKey(x => x.BuyerUserId).OnDelete(DeleteBehavior.Restrict);
            b.HasMany(x => x.ContactUnlocksAsTarget).WithOne(x => x.TargetUser).HasForeignKey(x => x.TargetUserId).OnDelete(DeleteBehavior.Restrict);
        }
    }

    public class UserRoleConfig : IEntityTypeConfiguration<UserRole>
    {
        public void Configure(EntityTypeBuilder<UserRole> b)
        {
            b.ToTable("user_roles");
            b.HasKey(x => new { x.UserId, x.Role });

            b.Property(x => x.Role).HasConversion<string>().HasMaxLength(30).IsRequired();
            b.Property(x => x.CreatedAt).HasDefaultValueSql(SqlDefaults.NowUtc);

            b.HasIndex(x => x.Role);
        }
    }

    public class UserKycConfig : IEntityTypeConfiguration<UserKyc>
    {
        public void Configure(EntityTypeBuilder<UserKyc> b)
        {
            b.ToTable("user_kyc");
            b.HasKey(x => x.Id);

            b.Property(x => x.KycType).HasMaxLength(30).IsRequired();
            b.Property(x => x.KycNumber).HasMaxLength(50).IsRequired();

            b.Property(x => x.VerifiedStatus).HasConversion<string>().HasMaxLength(20).IsRequired();
            b.Property(x => x.CreatedAt).HasDefaultValueSql(SqlDefaults.NowUtc);

            b.HasIndex(x => x.UserId);
        }
    }

    public class AddressConfig : IEntityTypeConfiguration<Address>
    {
        public void Configure(EntityTypeBuilder<Address> b)
        {
            b.ToTable("addresses");
            b.HasKey(x => x.Id);

            b.Property(x => x.Line1).HasMaxLength(200).IsRequired();
            b.Property(x => x.Line2).HasMaxLength(200);
            b.Property(x => x.City).HasMaxLength(100).IsRequired();
            b.Property(x => x.State).HasMaxLength(100).IsRequired();
            b.Property(x => x.PostalCode).HasMaxLength(20).IsRequired();
            b.Property(x => x.Country).HasMaxLength(60).HasDefaultValue("India");

            b.Property(x => x.Latitude).HasPrecision(9, 6);
            b.Property(x => x.Longitude).HasPrecision(9, 6);

            b.Property(x => x.CreatedAt).HasDefaultValueSql(SqlDefaults.NowUtc);

            b.HasIndex(x => new { x.City, x.State });
        }
    }

    public class UserAddressConfig : IEntityTypeConfiguration<UserAddress>
    {
        public void Configure(EntityTypeBuilder<UserAddress> b)
        {
            b.ToTable("user_addresses");
            b.HasKey(x => new { x.UserId, x.AddressId });

            b.Property(x => x.Label).HasMaxLength(50);
            b.Property(x => x.IsDefault).HasDefaultValue(false);
            b.Property(x => x.CreatedAt).HasDefaultValueSql(SqlDefaults.NowUtc);

            b.HasOne(x => x.User).WithMany(x => x.Addresses).HasForeignKey(x => x.UserId).OnDelete(DeleteBehavior.Cascade);
            b.HasOne(x => x.Address).WithMany(x => x.Users).HasForeignKey(x => x.AddressId).OnDelete(DeleteBehavior.Cascade);

            b.HasIndex(x => new { x.UserId, x.IsDefault });
        }
    }

    public class UserContactConfig : IEntityTypeConfiguration<UserContact>
    {
        public void Configure(EntityTypeBuilder<UserContact> b)
        {
            b.ToTable("user_contacts");
            b.HasKey(x => x.Id);

            b.Property(x => x.ContactType).HasConversion<string>().HasMaxLength(30).IsRequired();
            b.Property(x => x.Name).HasMaxLength(150);
            b.Property(x => x.Phone).HasMaxLength(20).IsRequired();
            b.Property(x => x.IsPublic).HasDefaultValue(false);
            b.Property(x => x.CreatedAt).HasDefaultValueSql(SqlDefaults.NowUtc);

            b.HasIndex(x => x.UserId);
            b.HasIndex(x => new { x.UserId, x.Phone }).IsUnique(); // avoid duplicates per user
        }
    }

    public class ProductConfig : IEntityTypeConfiguration<Product>
    {
        public void Configure(EntityTypeBuilder<Product> b)
        {
            b.ToTable("products");
            b.HasKey(x => x.Id);

            b.Property(x => x.Name).HasMaxLength(150).IsRequired();
            b.Property(x => x.Category).HasMaxLength(80);
            b.Property(x => x.Unit).HasMaxLength(20).HasDefaultValue("UNIT");
            b.Property(x => x.IsActive).HasDefaultValue(true);
            b.Property(x => x.CreatedAt).HasDefaultValueSql(SqlDefaults.NowUtc);

            b.HasIndex(x => new { x.Name, x.IsActive });
        }
    }

    public class VendorListingConfig : IEntityTypeConfiguration<VendorListing>
    {
        public void Configure(EntityTypeBuilder<VendorListing> b)
        {
            b.ToTable("vendor_listings");
            b.HasKey(x => x.Id);

            b.Property(x => x.AvailableQty).HasPrecision(18, 3).IsRequired();
            b.Property(x => x.MinOrderQty).HasPrecision(18, 3).HasDefaultValue(0m).IsRequired();

            b.Property(x => x.PricePerUnit).HasPrecision(18, 2).IsRequired();
            b.Property(x => x.Currency).HasMaxLength(10).HasDefaultValue("INR").IsRequired();

            b.Property(x => x.IsActive).HasDefaultValue(true);
            b.Property(x => x.CreatedAt).HasDefaultValueSql(SqlDefaults.NowUtc);

            b.HasOne(x => x.VendorUser).WithMany(x => x.VendorListings).HasForeignKey(x => x.VendorUserId).OnDelete(DeleteBehavior.Restrict);
            b.HasOne(x => x.Product).WithMany(x => x.VendorListings).HasForeignKey(x => x.ProductId).OnDelete(DeleteBehavior.Restrict);
            b.HasOne(x => x.ListingAddress).WithMany(x => x.VendorListings).HasForeignKey(x => x.ListingAddressId).OnDelete(DeleteBehavior.SetNull);

            b.HasIndex(x => x.VendorUserId);
            b.HasIndex(x => new { x.ProductId, x.IsActive });
            b.HasIndex(x => x.ListingAddressId);
        }
    }

    public class OrderConfig : IEntityTypeConfiguration<Order>
    {
        public void Configure(EntityTypeBuilder<Order> b)
        {
            b.ToTable("orders");
            b.HasKey(x => x.Id);

            b.Property(x => x.Status).HasConversion<string>().HasMaxLength(20).IsRequired();
            b.Property(x => x.Notes).HasMaxLength(2000);

            b.Property(x => x.CreatedAt).HasDefaultValueSql(SqlDefaults.NowUtc);
            b.Property(x => x.UpdatedAt).HasDefaultValueSql(SqlDefaults.NowUtc);

            b.HasOne(x => x.BuyerUser).WithMany(x => x.BuyerOrders).HasForeignKey(x => x.BuyerUserId).OnDelete(DeleteBehavior.Restrict);
            b.HasOne(x => x.VendorUser).WithMany(x => x.VendorOrders).HasForeignKey(x => x.VendorUserId).OnDelete(DeleteBehavior.Restrict);
            b.HasOne(x => x.DeliveryAddress).WithMany(x => x.OrdersDeliverTo).HasForeignKey(x => x.DeliveryAddressId).OnDelete(DeleteBehavior.Restrict);

            b.HasMany(x => x.Items).WithOne(x => x.Order).HasForeignKey(x => x.OrderId).OnDelete(DeleteBehavior.Cascade);

            b.HasIndex(x => x.BuyerUserId);
            b.HasIndex(x => x.VendorUserId);
            b.HasIndex(x => x.Status);
            b.HasIndex(x => x.CreatedAt);
        }
    }

    public class OrderItemConfig : IEntityTypeConfiguration<OrderItem>
    {
        public void Configure(EntityTypeBuilder<OrderItem> b)
        {
            b.ToTable("order_items");
            b.HasKey(x => x.Id);

            b.Property(x => x.Qty).HasPrecision(18, 3).IsRequired();
            b.Property(x => x.UnitPrice).HasPrecision(18, 2).IsRequired();
            b.Property(x => x.LineAmount).HasPrecision(18, 2).IsRequired();
            b.Property(x => x.Currency).HasMaxLength(10).HasDefaultValue("INR").IsRequired();

            b.Property(x => x.CreatedAt).HasDefaultValueSql(SqlDefaults.NowUtc);

            b.HasOne(x => x.VendorListing).WithMany(x => x.OrderItems).HasForeignKey(x => x.VendorListingId).OnDelete(DeleteBehavior.Restrict);
            b.HasOne(x => x.Product).WithMany(x => x.OrderItems).HasForeignKey(x => x.ProductId).OnDelete(DeleteBehavior.Restrict);

            b.HasIndex(x => x.OrderId);
            b.HasIndex(x => x.VendorListingId);
            b.HasIndex(x => x.ProductId);
        }
    }

    public class PaymentConfig : IEntityTypeConfiguration<Payment>
    {
        public void Configure(EntityTypeBuilder<Payment> b)
        {
            b.ToTable("payments");
            b.HasKey(x => x.Id);

            b.Property(x => x.Amount).HasPrecision(18, 2).IsRequired();
            b.Property(x => x.Currency).HasMaxLength(10).HasDefaultValue("INR").IsRequired();

            b.Property(x => x.Status).HasConversion<string>().HasMaxLength(20).IsRequired();
            b.Property(x => x.Provider).HasMaxLength(50);
            b.Property(x => x.ProviderRef).HasMaxLength(100);

            b.Property(x => x.CreatedAt).HasDefaultValueSql(SqlDefaults.NowUtc);

            b.HasOne(x => x.Order).WithMany(x => x.Payments).HasForeignKey(x => x.OrderId).OnDelete(DeleteBehavior.Cascade);
            b.HasOne(x => x.PayerUser).WithMany(x => x.PaymentsAsPayer).HasForeignKey(x => x.PayerUserId).OnDelete(DeleteBehavior.Restrict);

            b.HasIndex(x => x.OrderId);
            b.HasIndex(x => x.PayerUserId);
            b.HasIndex(x => x.Status);
        }
    }

    public class VehicleConfig : IEntityTypeConfiguration<Vehicle>
    {
        public void Configure(EntityTypeBuilder<Vehicle> b)
        {
            b.ToTable("vehicles");
            b.HasKey(x => x.Id);

            b.Property(x => x.VehicleType).HasMaxLength(50).IsRequired();
            b.Property(x => x.RegNumber).HasMaxLength(30);

            b.Property(x => x.MaxVolume).HasPrecision(18, 3);
            b.Property(x => x.MaxWeight).HasPrecision(18, 3);

            b.Property(x => x.IsActive).HasDefaultValue(true);
            b.Property(x => x.CreatedAt).HasDefaultValueSql(SqlDefaults.NowUtc);

            b.HasOne(x => x.TransporterUser).WithMany(x => x.Vehicles).HasForeignKey(x => x.TransporterUserId).OnDelete(DeleteBehavior.Restrict);

            b.HasIndex(x => x.TransporterUserId);
            b.HasIndex(x => new { x.TransporterUserId, x.IsActive });
        }
    }

    public class TransportRouteConfig : IEntityTypeConfiguration<TransportRoute>
    {
        public void Configure(EntityTypeBuilder<TransportRoute> b)
        {
            b.ToTable("transport_routes");
            b.HasKey(x => x.Id);

            b.Property(x => x.FromCity).HasMaxLength(100);
            b.Property(x => x.ToCity).HasMaxLength(100);

            // DateOnly mapping to SQL Server date
            b.Property(x => x.DepartDate).HasColumnType("date");

            b.Property(x => x.CapacityWeightAvailable).HasPrecision(18, 3);
            b.Property(x => x.CapacityVolumeAvailable).HasPrecision(18, 3);

            b.Property(x => x.BasePrice).HasPrecision(18, 2);
            b.Property(x => x.PriceModel).HasConversion<string>().HasMaxLength(30).IsRequired();
            b.Property(x => x.Currency).HasMaxLength(10).HasDefaultValue("INR").IsRequired();

            b.Property(x => x.IsActive).HasDefaultValue(true);
            b.Property(x => x.CreatedAt).HasDefaultValueSql(SqlDefaults.NowUtc);

            b.HasOne(x => x.TransporterUser).WithMany(x => x.TransportRoutes).HasForeignKey(x => x.TransporterUserId).OnDelete(DeleteBehavior.Restrict);
            b.HasOne(x => x.Vehicle).WithMany(x => x.Routes).HasForeignKey(x => x.VehicleId).OnDelete(DeleteBehavior.SetNull);

            b.HasOne(x => x.FromAddress).WithMany(x => x.RoutesFrom).HasForeignKey(x => x.FromAddressId).OnDelete(DeleteBehavior.SetNull);
            b.HasOne(x => x.ToAddress).WithMany(x => x.RoutesTo).HasForeignKey(x => x.ToAddressId).OnDelete(DeleteBehavior.SetNull);

            b.HasIndex(x => x.TransporterUserId);
            b.HasIndex(x => new { x.IsActive, x.DepartDate });
            b.HasIndex(x => new { x.FromCity, x.ToCity });
        }
    }

    public class ShipmentConfig : IEntityTypeConfiguration<Shipment>
    {
        public void Configure(EntityTypeBuilder<Shipment> b)
        {
            b.ToTable("shipments");
            b.HasKey(x => x.Id);

            b.Property(x => x.Status).HasConversion<string>().HasMaxLength(20).IsRequired();

            b.Property(x => x.QuotedPrice).HasPrecision(18, 2);
            b.Property(x => x.FinalPrice).HasPrecision(18, 2);
            b.Property(x => x.Currency).HasMaxLength(10).HasDefaultValue("INR").IsRequired();

            b.Property(x => x.CreatedAt).HasDefaultValueSql(SqlDefaults.NowUtc);

            b.HasOne(x => x.Order).WithMany(x => x.Shipments).HasForeignKey(x => x.OrderId).OnDelete(DeleteBehavior.Cascade);
            b.HasOne(x => x.TransporterUser).WithMany(x => x.ShipmentsAsTransporter).HasForeignKey(x => x.TransporterUserId).OnDelete(DeleteBehavior.Restrict);

            b.HasOne(x => x.TransportRoute).WithMany(x => x.Shipments).HasForeignKey(x => x.TransportRouteId).OnDelete(DeleteBehavior.SetNull);
            b.HasOne(x => x.Vehicle).WithMany(x => x.Shipments).HasForeignKey(x => x.VehicleId).OnDelete(DeleteBehavior.SetNull);

            b.HasOne(x => x.PickupAddress).WithMany(x => x.ShipmentsPickup).HasForeignKey(x => x.PickupAddressId).OnDelete(DeleteBehavior.Restrict);
            b.HasOne(x => x.DropAddress).WithMany(x => x.ShipmentsDrop).HasForeignKey(x => x.DropAddressId).OnDelete(DeleteBehavior.Restrict);

            b.HasMany(x => x.ShipmentContacts).WithOne(x => x.Shipment).HasForeignKey(x => x.ShipmentId).OnDelete(DeleteBehavior.Cascade);

            b.HasIndex(x => x.OrderId);
            b.HasIndex(x => x.TransporterUserId);
            b.HasIndex(x => x.Status);
        }
    }

    public class ShipmentContactConfig : IEntityTypeConfiguration<ShipmentContact>
    {
        public void Configure(EntityTypeBuilder<ShipmentContact> b)
        {
            b.ToTable("shipment_contacts");
            b.HasKey(x => x.Id);

            b.Property(x => x.ContactName).HasMaxLength(150).IsRequired();
            b.Property(x => x.Phone).HasMaxLength(20).IsRequired();
            b.Property(x => x.Role).HasMaxLength(30).HasDefaultValue("Driver").IsRequired();

            b.Property(x => x.IsSharedWithBuyer).HasDefaultValue(false);
            b.Property(x => x.CreatedAt).HasDefaultValueSql(SqlDefaults.NowUtc);

            b.HasIndex(x => x.ShipmentId);
        }
    }

    public class ContactUnlockConfig : IEntityTypeConfiguration<ContactUnlock>
    {
        public void Configure(EntityTypeBuilder<ContactUnlock> b)
        {
            b.ToTable("contact_unlocks");
            b.HasKey(x => x.Id);

            b.Property(x => x.UnlockReason).HasMaxLength(30).HasDefaultValue("Payment").IsRequired();
            b.Property(x => x.UnlockedAt).HasDefaultValueSql(SqlDefaults.NowUtc);
            b.Property(x => x.CreatedAt).HasDefaultValueSql(SqlDefaults.NowUtc);

            b.HasOne(x => x.BuyerUser).WithMany(x => x.ContactUnlocksAsBuyer).HasForeignKey(x => x.BuyerUserId).OnDelete(DeleteBehavior.Restrict);
            b.HasOne(x => x.TargetUser).WithMany(x => x.ContactUnlocksAsTarget).HasForeignKey(x => x.TargetUserId).OnDelete(DeleteBehavior.Restrict);
            b.HasOne(x => x.Order).WithMany().HasForeignKey(x => x.OrderId).OnDelete(DeleteBehavior.SetNull);

            b.HasIndex(x => new { x.BuyerUserId, x.TargetUserId, x.ExpiresAt });
        }
    }
}
