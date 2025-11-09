using IgnaCheck.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace IgnaCheck.Infrastructure.Data.Configurations;

public class OrganizationConfiguration : IEntityTypeConfiguration<Organization>
{
    public void Configure(EntityTypeBuilder<Organization> builder)
    {
        builder.Property(t => t.Name)
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(t => t.Domain)
            .HasMaxLength(200);

        builder.Property(t => t.SubscriptionTier)
            .HasMaxLength(50);

        builder.Property(t => t.IsActive)
            .IsRequired();

        // Index for faster lookups
        builder.HasIndex(t => t.Domain);
        builder.HasIndex(t => t.IsActive);
    }
}
