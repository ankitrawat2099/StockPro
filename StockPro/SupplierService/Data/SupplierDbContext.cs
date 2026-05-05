using Microsoft.EntityFrameworkCore;

    public class SupplierDbContext : DbContext
    {
        public SupplierDbContext(DbContextOptions<SupplierDbContext> options)
            : base(options)
        {
        }

        public DbSet<Supplier> Suppliers { get; set; }
    }
