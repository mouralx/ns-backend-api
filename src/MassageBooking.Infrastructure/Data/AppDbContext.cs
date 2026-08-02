using MassageBooking.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace MassageBooking.Infrastructure.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    public DbSet<User> Users => Set<User>();
    public DbSet<ServiceType> ServiceTypes => Set<ServiceType>();
    public DbSet<Appointment> Appointments => Set<Appointment>();
    public DbSet<Notification> Notifications => Set<Notification>();
    public DbSet<AvailabilityRule> AvailabilityRules => Set<AvailabilityRule>();
    public DbSet<AvailabilityBlock> AvailabilityBlocks => Set<AvailabilityBlock>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Apply all configurations from the assembly
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
    }
}
