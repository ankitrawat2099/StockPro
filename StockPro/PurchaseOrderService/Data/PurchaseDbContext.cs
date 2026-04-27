using Microsoft.EntityFrameworkCore;

public class PurchaseDbContext : DbContext
{
    public PurchaseDbContext(DbContextOptions<PurchaseDbContext> options)
        : base(options)
    {
    }

    public DbSet<PurchaseOrder> PurchaseOrders { get; set; }

    public DbSet<POLineItem> POLineItems { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<PurchaseOrder>()
            .HasMany(p => p.Items)
            .WithOne()
            .HasForeignKey(p => p.PoId);
    }
}