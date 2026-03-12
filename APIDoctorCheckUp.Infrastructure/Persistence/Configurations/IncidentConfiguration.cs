using APIDoctorCheckUp.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace APIDoctorCheckUp.Infrastructure.Persistence.Configurations;

public class IncidentConfiguration : IEntityTypeConfiguration<Incident>
{
    public void Configure(EntityTypeBuilder<Incident> builder)
    {
        builder.HasKey(i => i.Id);

        builder.Property(i => i.TriggerReason)
            .HasConversion<string>()
            .HasMaxLength(30);

        // Ignore computed properties — they derive from persisted data
        // and do not need their own database columns
        builder.Ignore(i => i.IsOpen);
        builder.Ignore(i => i.Duration);

        builder.HasIndex(i => new { i.EndpointId, i.ResolvedAt });
    }
}
