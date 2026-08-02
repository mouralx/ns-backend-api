using MassageBooking.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MassageBooking.Infrastructure.Data.Configurations;

public class ServiceTypeConfiguration : IEntityTypeConfiguration<ServiceType>
{
    public void Configure(EntityTypeBuilder<ServiceType> builder)
    {
        builder.HasKey(s => s.Id);

        builder.Property(s => s.Name)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(s => s.DurationMin)
            .IsRequired();

        builder.Property(s => s.Description)
            .IsRequired()
            .HasMaxLength(500);

        builder.Property(s => s.IsActive)
            .IsRequired()
            .HasDefaultValue(true);
    }
}
