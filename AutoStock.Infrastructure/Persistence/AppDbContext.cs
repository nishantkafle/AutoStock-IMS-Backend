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

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        // Fast login lookup 
        builder.Entity<User>()
            .HasIndex(u => u.Email)
            .IsUnique();

        // Vehicle number must be unique 
        builder.Entity<Vehicle>()
            .HasIndex(v => v.VehicleNumber)
            .IsUnique();

        // Vehicle belongs to a User
        builder.Entity<Vehicle>()
            .HasOne(v => v.Customer)
            .WithMany()
            .HasForeignKey(v => v.CustomerId)
            .OnDelete(DeleteBehavior.Cascade);

        // Appointment belongs to a User (customer)
        builder.Entity<Appointment>()
            .HasOne(a => a.Customer)
            .WithMany()
            .HasForeignKey(a => a.CustomerId)
            .OnDelete(DeleteBehavior.Cascade);

        // Appointment optionally linked to a vehicle
        builder.Entity<Appointment>()
            .HasOne(a => a.Vehicle)
            .WithMany()
            .HasForeignKey(a => a.VehicleId)
            .OnDelete(DeleteBehavior.SetNull);

        // PartRequest belongs to a User
        builder.Entity<PartRequest>()
            .HasOne(r => r.Customer)
            .WithMany()
            .HasForeignKey(r => r.CustomerId)
            .OnDelete(DeleteBehavior.Cascade);

        // Review belongs to a User
        builder.Entity<Review>()
            .HasOne(r => r.Customer)
            .WithMany()
            .HasForeignKey(r => r.CustomerId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
