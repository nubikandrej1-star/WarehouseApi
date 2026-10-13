using Microsoft.EntityFrameworkCore;
using WarehouseApi.Domain.Entities;

namespace WarehouseApi.Data;

public class WarehouseDbContext : DbContext
{
    public DbSet<Product> Products => Set<Product>();
    public DbSet<StockTransaction> StockTransactions => Set<StockTransaction>();
    public DbSet<Supplier> Suppliers => Set<Supplier>();
    public DbSet<Shipment> Shipments => Set<Shipment>();

    public WarehouseDbContext(DbContextOptions<WarehouseDbContext> options)
        : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Product>(entity =>
        {
            entity.HasKey(p => p.Id);
            entity.Property(p => p.Name).HasMaxLength(200).IsRequired();
            entity.Property(p => p.Sku).HasMaxLength(50).IsRequired();
            entity.HasIndex(p => p.Sku).IsUnique();
            entity.Property(p => p.Description).HasMaxLength(1000);
            entity.Property(p => p.Price).HasPrecision(18, 2).IsRequired();
            entity.Property(p => p.Category).HasMaxLength(100).IsRequired();
            entity.Property(p => p.StockQuantity).IsRequired();
            entity.Property(p => p.IsActive).HasDefaultValue(true);
            entity.Property(p => p.CreatedAt).HasDefaultValueSql("GETUTCDATE()");

            entity.HasMany(p => p.Transactions)
                  .WithOne(t => t.Product)
                  .HasForeignKey(t => t.ProductId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<StockTransaction>(entity =>
        {
            entity.HasKey(t => t.Id);
            entity.Property(t => t.QuantityChange).IsRequired();
            entity.Property(t => t.Reason).HasMaxLength(1000).IsRequired();
            entity.Property(t => t.Timestamp).HasDefaultValueSql("GETUTCDATE()");
        });

        modelBuilder.Entity<Supplier>(entity =>
        {
            entity.HasKey(s => s.Id);
            entity.Property(s => s.CompanyName).HasMaxLength(200).IsRequired();
            entity.Property(s => s.EdrpouCode).HasMaxLength(10).IsRequired();
            entity.HasIndex(s => s.EdrpouCode).IsUnique();
            entity.Property(s => s.ContactPerson).HasMaxLength(100);
            entity.Property(s => s.Phone).HasMaxLength(20);
            entity.Property(s => s.Email).HasMaxLength(100);
            entity.Property(s => s.Address).HasMaxLength(500);
            entity.Property(s => s.IsActive).HasDefaultValue(true);
            entity.Property(s => s.CreatedAt).HasDefaultValueSql("GETUTCDATE()");
        });

        modelBuilder.Entity<Shipment>(entity =>
        {
            entity.HasKey(shipment => shipment.Id);
            entity.Property(shipment => shipment.TrackingNumber).HasMaxLength(50).IsRequired();
            entity.HasIndex(shipment => shipment.TrackingNumber).IsUnique();
            entity.Property(shipment => shipment.RecipientName).HasMaxLength(150).IsRequired();
            entity.Property(shipment => shipment.DestinationAddress).HasMaxLength(300).IsRequired();
            entity.Property(shipment => shipment.Status).HasMaxLength(20).IsRequired().HasDefaultValue("Created");
            entity.Property(shipment => shipment.Quantity).IsRequired();
            entity.Property(shipment => shipment.CreatedAt).HasDefaultValueSql("GETUTCDATE()");
            entity.HasOne(shipment => shipment.Product)
                .WithMany()
                .HasForeignKey(shipment => shipment.ProductId)
                .OnDelete(DeleteBehavior.Restrict)
                .IsRequired();
        });
    }
}