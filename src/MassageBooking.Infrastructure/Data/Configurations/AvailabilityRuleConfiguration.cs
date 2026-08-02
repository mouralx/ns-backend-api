using MassageBooking.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MassageBooking.Infrastructure.Data.Configurations;

public class AvailabilityRuleConfiguration : IEntityTypeConfiguration<AvailabilityRule>
{
    public void Configure(EntityTypeBuilder<AvailabilityRule> builder)
    {
        builder.HasKey(r => r.Id);

        builder.Property(r => r.DayOfWeek)
            .IsRequired();

        builder.Property(r => r.StartTime)
            .IsRequired();

        builder.Property(r => r.EndTime)
            .IsRequired();

        builder.Property(r => r.IsActive)
            .IsRequired()
            .HasDefaultValue(true);

        // Index for querying rules by therapist
        builder.HasIndex(r => new { r.TherapistId, r.DayOfWeek });

        // Foreign key
        builder.HasOne(r => r.Therapist)
            .WithMany(u => u.AvailabilityRules)
            .HasForeignKey(r => r.TherapistId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
