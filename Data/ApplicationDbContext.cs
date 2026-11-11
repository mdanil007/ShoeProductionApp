using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using ShoeProductionApp.Entities;

namespace ShoeProductionApp.Data;

public class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : IdentityDbContext<ApplicationUser>(options)
{
    public DbSet<Client> Clients => Set<Client>();
    public DbSet<Model> Models => Set<Model>();
    public DbSet<Color> Colors => Set<Color>();
    public DbSet<Leather> Leathers => Set<Leather>();
    public DbSet<Lining> Linings => Set<Lining>();
    public DbSet<Sole> Soles => Set<Sole>();
    public DbSet<Product> Products => Set<Product>();
    public DbSet<Order> Orders => Set<Order>();
    public DbSet<OrderSize> OrderSizes => Set<OrderSize>();
    public DbSet<OrderStatusHistory> OrderStatusHistories => Set<OrderStatusHistory>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);
        builder.Entity<Client>(e => { e.Property(x=>x.Name).IsRequired(); e.Property(x=>x.Type).IsRequired(); });
        ConfigureCodeEntity<Color>(builder, 2, "CK_Color_Code");
        ConfigureCodeEntity<Leather>(builder, 1, "CK_Leather_Code");
        ConfigureCodeEntity<Lining>(builder, 1, "CK_Lining_Code");
        ConfigureCodeEntity<Sole>(builder, 3, "CK_Sole_Code");
        builder.Entity<Model>(e => { e.Property(x=>x.Name).IsRequired(); e.Property(x=>x.Code).HasMaxLength(3).IsRequired(); e.HasIndex(x=>x.Code).IsUnique(); e.ToTable(t=>t.HasCheckConstraint("CK_Model_Code", "\"Code\" ~ '^[0-9]{3}$'")); });
        builder.Entity<Product>(e =>
        {
            e.Property(x => x.ImageFileName).HasMaxLength(100);
            e.HasIndex(x=>new { x.Gender,x.ModelId,x.ColorId,x.LeatherId,x.LiningId,x.SoleId }).IsUnique();
            e.ToTable(t=>t.HasCheckConstraint("CK_Product_Gender", "\"Gender\" IN (6, 7, 8, 9)"));
            e.HasOne(x=>x.Model).WithMany(x=>x.Products).HasForeignKey(x=>x.ModelId).OnDelete(DeleteBehavior.Restrict);
            e.HasOne(x=>x.Color).WithMany(x=>x.Products).HasForeignKey(x=>x.ColorId).OnDelete(DeleteBehavior.Restrict);
            e.HasOne(x=>x.Leather).WithMany(x=>x.Products).HasForeignKey(x=>x.LeatherId).OnDelete(DeleteBehavior.Restrict);
            e.HasOne(x=>x.Lining).WithMany(x=>x.Products).HasForeignKey(x=>x.LiningId).OnDelete(DeleteBehavior.Restrict);
            e.HasOne(x=>x.Sole).WithMany(x=>x.Products).HasForeignKey(x=>x.SoleId).OnDelete(DeleteBehavior.Restrict);
        });
        builder.Entity<Order>(e =>
        {
            e.Property(x=>x.CreatedAt).HasColumnType("timestamp with time zone"); e.Property(x=>x.PlannedShipmentDate).HasColumnType("date"); e.Property(x=>x.ActualShipmentDate).HasColumnType("timestamp with time zone");
            e.HasOne(x=>x.Client).WithMany(x=>x.Orders).HasForeignKey(x=>x.ClientId).OnDelete(DeleteBehavior.Restrict);
            e.HasOne(x=>x.Product).WithMany(x=>x.Orders).HasForeignKey(x=>x.ProductId).OnDelete(DeleteBehavior.Restrict);
            e.HasOne(x=>x.CreatedByUser).WithMany().HasForeignKey(x=>x.CreatedByUserId).OnDelete(DeleteBehavior.Restrict);
        });
        builder.Entity<OrderSize>(e => { e.HasKey(x=>new {x.OrderId,x.Size}); e.Property(x=>x.Size).HasMaxLength(2).IsRequired(); e.ToTable(t=>{t.HasCheckConstraint("CK_OrderSize_Size", "\"Size\" ~ '^(3[0-9]|[4-5][0-9]|60)$'"); t.HasCheckConstraint("CK_OrderSize_Count", "\"Count\" > 0");}); e.HasOne(x=>x.Order).WithMany(x=>x.Sizes).HasForeignKey(x=>x.OrderId).OnDelete(DeleteBehavior.Cascade); });
        builder.Entity<OrderStatusHistory>(e=>{e.Property(x=>x.ChangedAt).HasColumnType("timestamp with time zone"); e.HasOne(x=>x.Order).WithMany(x=>x.StatusHistory).HasForeignKey(x=>x.OrderId).OnDelete(DeleteBehavior.Cascade); e.HasOne(x=>x.ChangedByUser).WithMany().HasForeignKey(x=>x.ChangedByUserId).OnDelete(DeleteBehavior.Restrict);});
    }

    private static void ConfigureCodeEntity<T>(ModelBuilder builder,int length,string constraint) where T:class
    {
        builder.Entity<T>(e=>{ e.Property<string>("Name").IsRequired(); e.Property<string>("Code").HasMaxLength(length).IsRequired(); e.HasIndex("Code").IsUnique(); e.ToTable(t=>t.HasCheckConstraint(constraint,$"\"Code\" ~ '^[0-9]{{{length}}}$'")); });
    }
}
