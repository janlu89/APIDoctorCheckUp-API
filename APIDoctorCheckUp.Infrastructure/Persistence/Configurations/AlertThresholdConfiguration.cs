using APIDoctorCheckUp.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace APIDoctorCheckUp.Infrastructure.Persistence.Configurations;

public class AlertThresholdConfiguration : IEntityTypeConfiguration<AlertThreshold>
{
    public void Configure(EntityTypeBuilder<AlertThreshold> builder)
    {
        builder.HasKey(t => t.Id);

        builder.Property(t => t.ResponseTimeWarningMs)
            .HasDefaultValue(1000);

        builder.Property(t => t.ResponseTimeCriticalMs)
            .HasDefaultValue(3000);

        builder.Property(t => t.ConsecutiveFailuresDown)
            .HasDefaultValue(3);
    }
}
