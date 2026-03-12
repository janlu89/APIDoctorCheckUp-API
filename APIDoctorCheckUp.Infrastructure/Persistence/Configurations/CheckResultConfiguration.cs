using APIDoctorCheckUp.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace APIDoctorCheckUp.Infrastructure.Persistence.Configurations;

public class CheckResultConfiguration : IEntityTypeConfiguration<CheckResult>
{
    public void Configure(EntityTypeBuilder<CheckResult> builder)
    {
        builder.HasKey(c => c.Id);

        builder.Property(c => c.ErrorMessage)
            .HasMaxLength(1000);

        // Index on EndpointId + CheckedAt DESC — the most common query pattern
        // is "give me the last N checks for endpoint X", so this index is critical
        // for performance once CheckResults grows to thousands of rows per endpoint
        builder.HasIndex(c => new { c.EndpointId, c.CheckedAt });
    }
}
