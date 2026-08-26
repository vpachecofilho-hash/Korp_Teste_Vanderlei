using BillingService.Models;
using Microsoft.EntityFrameworkCore;

namespace BillingService.Data;

public class BillingDbContext : DbContext
{
    public BillingDbContext(DbContextOptions<BillingDbContext> options)
        : base(options)
    {
    }

    public DbSet<Invoice> Invoices => Set<Invoice>();

    public DbSet<InvoiceItem> InvoiceItems => Set<InvoiceItem>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasSequence<long>("invoice_number_seq")
            .StartsAt(1)
            .IncrementsBy(1);

        modelBuilder.Entity<Invoice>(entity =>
        {
            entity.HasKey(invoice => invoice.Id);

            entity.HasIndex(invoice => invoice.Number)
                .IsUnique();

            entity.Property(invoice => invoice.Number)
                .HasDefaultValueSql("nextval('invoice_number_seq')");

            entity.Property(invoice => invoice.Status)
                .HasConversion<string>()
                .HasMaxLength(20);

            entity.HasMany(invoice => invoice.Items)
                .WithOne(item => item.Invoice)
                .HasForeignKey(item => item.InvoiceId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<InvoiceItem>(entity =>
        {
            entity.HasKey(item => item.Id);

            entity.Property(item => item.Quantity)
                .HasPrecision(18, 3);
        });
    }
}