using Microsoft.EntityFrameworkCore;
using AuthService.Entities;

namespace AuthService.Data;

public class AuthDbContext : DbContext
{
    public AuthDbContext(DbContextOptions<AuthDbContext> options)
        : base(options)
    {
    }

    public DbSet<AppUser> Users { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        var user = modelBuilder.Entity<AppUser>();

        user.HasIndex(u => u.Email)
            .IsUnique();
    }
}
