using MassageBooking.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MassageBooking.Infrastructure.Data.Configurations;

public class NotificationConfiguration : IEntityTypeConfiguration<Notification>
{
    public void Configure(EntityTypeBuilder<Notification> builder)
    {
        builder.HasKey(n => n.Id);

        builder.Property(n => n.Type)
            .IsRequired();

        builder.Property(n => n.Channel)
            .IsRequired();

        builder.Property(n => n.Status)
            .IsRequired();

        builder.Property(n => n.ErrorMessage)
            .HasMaxLength(1000);

        // Index for querying by user
        builder.HasIndex(n => n.UserId);

        // Foreign keys
        builder.HasOne(n => n.User)
            .WithMany(u => u.Notifications)
            .HasForeignKey(n => n.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(n => n.Appointment)
            .WithMany(a => a.Notifications)
            .HasForeignKey(n => n.AppointmentId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
