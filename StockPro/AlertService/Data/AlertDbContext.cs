using Microsoft.EntityFrameworkCore;

public class AlertDbContext : DbContext
{
    public AlertDbContext(DbContextOptions<AlertDbContext> options)
        : base(options)
    {
    }

    public DbSet<Alert> Alerts { get; set; }
}