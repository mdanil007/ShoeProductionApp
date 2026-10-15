using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using ShoeProductionApp.Entities;

namespace ShoeProductionApp.Data;

public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
{
    public ApplicationDbContext(
        DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    public DbSet<Client> Clients => Set<Client>();
    public DbSet<Model> Models => Set<Model>();
    public DbSet<Color> Colors => Set<Color>();
    public DbSet<Leather> Leathers => Set<Leather>();
    public DbSet<Lining> Linings => Set<Lining>();
    public DbSet<Product> Products => Set<Product>();
    public DbSet<Order> Orders => Set<Order>();
    public DbSet<OrderSize> OrderSizes => Set<OrderSize>();
    public DbSet<OrderStatusHistory> OrderStatusHistories
        => Set<OrderStatusHistory>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.Entity<Client>(entity =>
        {
            entity.Property(x => x.Name).IsRequired();
            entity.Property(x => x.Type).IsRequired();
        });

        builder.Entity<Color>(entity =>
        {
            entity.Property(x => x.Name).IsRequired();
            entity.Property(x => x.Code)
                .HasMaxLength(1)
                .IsRequired();

            entity.HasIndex(x => x.Code).IsUnique();

            entity.ToTable(t => t.HasCheckConstraint(
                "CK_Color_Code",
                "\"Code\" ~ '^[0-9]$'"));
        });

        builder.Entity<Leather>(entity =>
        {
            entity.Property(x => x.Name).IsRequired();
            entity.Property(x => x.Code)
                .HasMaxLength(1)
                .IsRequired();

            entity.HasIndex(x => x.Code).IsUnique();

            entity.ToTable(t => t.HasCheckConstraint(
                "CK_Leather_Code",
                "\"Code\" ~ '^[0-9]$'"));
        });

        builder.Entity<Lining>(entity =>
        {
            entity.Property(x => x.Name).IsRequired();
            entity.Property(x => x.Code)
                .HasMaxLength(1)
                .IsRequired();

            entity.HasIndex(x => x.Code).IsUnique();

            entity.ToTable(t => t.HasCheckConstraint(
                "CK_Lining_Code",
                "\"Code\" ~ '^[0-9]$'"));
        });

        builder.Entity<Model>(entity =>
        {
            entity.Property(x => x.Name).IsRequired();
            entity.Property(x => x.Code)
                .HasMaxLength(2)
                .IsRequired();

            entity.HasIndex(x => new { x.Code, x.ColorId })
                .IsUnique();

            entity.ToTable(t => t.HasCheckConstraint(
                "CK_Model_Code",
                "\"Code\" ~ '^[0-9]{2}$'"));

            entity.HasOne(x => x.Color)
                .WithMany(x => x.Models)
                .HasForeignKey(x => x.ColorId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        builder.Entity<Product>(entity =>
        {
            entity.HasIndex(x => new
            {
                x.Gender,
                x.LeatherId,
                x.LiningId,
                x.ModelId
            }).IsUnique();

            entity.ToTable(t => t.HasCheckConstraint(
                "CK_Product_Gender",
                "\"Gender\" IN (8, 9)"));

            entity.HasOne(x => x.Model)
                .WithMany(x => x.Products)
                .HasForeignKey(x => x.ModelId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(x => x.Leather)
                .WithMany(x => x.Products)
                .HasForeignKey(x => x.LeatherId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(x => x.Lining)
                .WithMany(x => x.Products)
                .HasForeignKey(x => x.LiningId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        builder.Entity<Order>(entity =>
        {
            entity.Property(x => x.CreatedAt)
                .HasColumnType("timestamp with time zone");

            entity.Property(x => x.PlannedShipmentDate)
                .HasColumnType("date");

            entity.Property(x => x.ActualShipmentDate)
                .HasColumnType("timestamp with time zone");

            entity.HasOne(x => x.Client)
                .WithMany(x => x.Orders)
                .HasForeignKey(x => x.ClientId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(x => x.Product)
                .WithMany(x => x.Orders)
                .HasForeignKey(x => x.ProductId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(x => x.CreatedByUser)
                .WithMany()
                .HasForeignKey(x => x.CreatedByUserId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        builder.Entity<OrderSize>(entity =>
        {
            entity.HasKey(x => new { x.OrderId, x.Size });

            entity.Property(x => x.Size)
                .HasMaxLength(2)
                .IsRequired();

            entity.ToTable(t =>
            {
                t.HasCheckConstraint(
                    "CK_OrderSize_Size",
                    "\"Size\" ~ '^(3[0-9]|[4-5][0-9]|60)$'");

                t.HasCheckConstraint(
                    "CK_OrderSize_Count",
                    "\"Count\" > 0");
            });

        entity.ToTable(t => t.HasCheckConstraint(
                "CK_OrderSize_Count",
                "\"Count\" > 0"));

            entity.HasOne(x => x.Order)
                .WithMany(x => x.Sizes)
                .HasForeignKey(x => x.OrderId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        builder.Entity<OrderStatusHistory>(entity =>
        {
            entity.Property(x => x.ChangedAt)
                .HasColumnType("timestamp with time zone");

            entity.HasOne(x => x.Order)
                .WithMany(x => x.StatusHistory)
                .HasForeignKey(x => x.OrderId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(x => x.ChangedByUser)
                .WithMany()
                .HasForeignKey(x => x.ChangedByUserId)
                .OnDelete(DeleteBehavior.Restrict);
        });
    }
}
