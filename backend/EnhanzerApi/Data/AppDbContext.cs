using Microsoft.EntityFrameworkCore;

namespace EnhanzerApi.Data;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<LocationDetail> LocationDetails => Set<LocationDetail>();
    public DbSet<PurchaseBill> PurchaseBills => Set<PurchaseBill>();
    public DbSet<PurchaseBillItem> PurchaseBillItems => Set<PurchaseBillItem>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<LocationDetail>()
            .HasIndex(l => l.LocationCode)
            .IsUnique();

        modelBuilder.Entity<PurchaseBill>()
            .HasMany(b => b.Items)
            .WithOne(i => i.PurchaseBill)
            .HasForeignKey(i => i.PurchaseBillId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
