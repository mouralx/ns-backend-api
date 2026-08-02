using MassageBooking.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MassageBooking.Infrastructure.Data.Configurations;

public class AvailabilityBlockConfiguration : IEntityTypeConfiguration<AvailabilityBlock>
{
    public void Configure(EntityTypeBuilder<AvailabilityBlock> builder)
    {
        builder.HasKey(b => b.Id);

        builder.Property(b => b.StartAt)
            .IsRequired();

        builder.Property(b => b.EndAt)
            .IsRequired();

        builder.Property(b => b.Reason)
            .HasMaxLength(500);

        builder.Property(b => b.RecurrenceRule)
            .HasMaxLength(255);

        // Index for querying blocks by therapist and date
        builder.HasIndex(b => new { b.TherapistId, b.StartAt, b.EndAt });

        // Foreign key
        builder.HasOne(b => b.Therapist)
            .WithMany(u => u.AvailabilityBlocks)
            .HasForeignKey(b => b.TherapistId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
