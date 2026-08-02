using MassageBooking.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MassageBooking.Infrastructure.Data.Configurations;

public class AppointmentConfiguration : IEntityTypeConfiguration<Appointment>
{
    public void Configure(EntityTypeBuilder<Appointment> builder)
    {
        builder.HasKey(a => a.Id);

        builder.Property(a => a.ScheduledAt)
            .IsRequired();

        builder.Property(a => a.DurationMin)
            .IsRequired();

        builder.Property(a => a.Status)
            .IsRequired();

        builder.Property(a => a.ConfirmationStatus)
            .IsRequired();

        builder.Property(a => a.Notes)
            .HasMaxLength(1000);

        builder.Property(a => a.CancellationReason)
            .HasMaxLength(500);

        // Composite index for availability queries
        builder.HasIndex(a => new { a.TherapistId, a.ScheduledAt, a.Status });

        // Index for date range queries
        builder.HasIndex(a => new { a.ScheduledAt, a.Status });

        // Foreign keys
        builder.HasOne(a => a.Client)
            .WithMany(u => u.ClientAppointments)
            .HasForeignKey(a => a.ClientId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(a => a.Therapist)
            .WithMany(u => u.TherapistAppointments)
            .HasForeignKey(a => a.TherapistId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(a => a.ServiceType)
            .WithMany(s => s.Appointments)
            .HasForeignKey(a => a.ServiceTypeId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
