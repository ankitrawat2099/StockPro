using Microsoft.EntityFrameworkCore;

public class ReportDbContext : DbContext
{
    public ReportDbContext(DbContextOptions<ReportDbContext> options)
        : base(options)
    {
    }

    public DbSet<InventorySnapshot> InventorySnapshots { get; set; }
}