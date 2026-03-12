using APIDoctorCheckUp.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace APIDoctorCheckUp.Infrastructure.Persistence.Configurations;

public class MonitoredEndpointConfiguration : IEntityTypeConfiguration<MonitoredEndpoint>
{
    public void Configure(EntityTypeBuilder<MonitoredEndpoint> builder)
    {
        builder.HasKey(e => e.Id);

        builder.Property(e => e.Name)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(e => e.Url)
            .IsRequired()
            .HasMaxLength(2048);

        builder.Property(e => e.ExpectedStatusCode)
            .HasDefaultValue(200);

        builder.Property(e => e.CheckIntervalSeconds)
            .HasDefaultValue(60);

        builder.Property(e => e.IsActive)
            .HasDefaultValue(true);

        builder.Property(e => e.CurrentStatus)
            .HasConversion<string>()
            .HasMaxLength(20);

        // One endpoint has many check results — deleting an endpoint
        // cascades and removes all its historical check data
        builder.HasMany(e => e.CheckResults)
            .WithOne(c => c.Endpoint)
            .HasForeignKey(c => c.EndpointId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(e => e.Incidents)
            .WithOne(i => i.Endpoint)
            .HasForeignKey(i => i.EndpointId)
            .OnDelete(DeleteBehavior.Cascade);

        // One-to-one relationship with AlertThreshold
        builder.HasOne(e => e.AlertThreshold)
            .WithOne(t => t.Endpoint)
            .HasForeignKey<AlertThreshold>(t => t.EndpointId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
