using Microsoft.EntityFrameworkCore;
using StockService.Models;

namespace StockService.Data;

public class StockDbContext : DbContext
{
    public StockDbContext(DbContextOptions<StockDbContext> options)
        : base(options)
    {
    }

    public DbSet<Product> Products => Set<Product>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Product>(entity =>
        {
            entity.HasKey(product => product.Id);

            entity.Property(product => product.Code)
                .IsRequired()
                .HasMaxLength(50);

            entity.HasIndex(product => product.Code)
                .IsUnique();

            entity.Property(product => product.Description)
                .IsRequired()
                .HasMaxLength(200);

            entity.Property(product => product.Stock)
                .HasPrecision(18, 3);
        });
    }
}