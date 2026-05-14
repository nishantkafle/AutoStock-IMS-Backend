using AutoStock.Domain.Entities;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace AutoStock.Infrastructure.Persistence;

public class AppDbContext(DbContextOptions<AppDbContext> options) : IdentityDbContext<User>(options)
{
    public DbSet<Part> Parts => Set<Part>();
    public DbSet<Vendor> Vendors => Set<Vendor>();
    public DbSet<Vehicle> Vehicles => Set<Vehicle>();
    public DbSet<Appointment> Appointments => Set<Appointment>();
    public DbSet<PartRequest> PartRequests => Set<PartRequest>();
    public DbSet<Review> Reviews => Set<Review>();
    public DbSet<Invoice> Invoices => Set<Invoice>();
    public DbSet<InvoiceItem> InvoiceItems => Set<InvoiceItem>();
    public DbSet<CreditSettlement> CreditSettlements => Set<CreditSettlement>();
    public DbSet<PurchaseInvoice> PurchaseInvoices => Set<PurchaseInvoice>();
    public DbSet<PurchaseInvoiceItem> PurchaseInvoiceItems => Set<PurchaseInvoiceItem>();
    public DbSet<SaleInvoice> SaleInvoices { get; set; }
    public DbSet<SaleInvoiceItem> SaleInvoiceItems { get; set; }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.Entity<User>()
            .HasIndex(u => u.Email)
            .IsUnique();

        builder.Entity<Vehicle>()
            .HasIndex(v => v.VehicleNumber)
            .IsUnique();

        builder.Entity<Vehicle>()
            .HasOne(v => v.Customer)
            .WithMany()
            .HasForeignKey(v => v.CustomerId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Entity<Appointment>()
            .HasOne(a => a.Customer)
            .WithMany()
            .HasForeignKey(a => a.CustomerId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Entity<Appointment>()
            .HasOne(a => a.Vehicle)
            .WithMany()
            .HasForeignKey(a => a.VehicleId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.Entity<PartRequest>()
            .HasOne(r => r.Customer)
            .WithMany()
            .HasForeignKey(r => r.CustomerId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Entity<Review>()
            .HasOne(r => r.Customer)
            .WithMany()
            .HasForeignKey(r => r.CustomerId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Entity<Invoice>()
            .HasOne(i => i.Staff)
            .WithMany()
            .HasForeignKey(i => i.StaffId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.Entity<InvoiceItem>()
            .HasOne(i => i.Invoice)
            .WithMany(inv => inv.Items)
            .HasForeignKey(i => i.InvoiceId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Entity<InvoiceItem>()
            .HasOne(i => i.Part)
            .WithMany()
            .HasForeignKey(i => i.PartId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}