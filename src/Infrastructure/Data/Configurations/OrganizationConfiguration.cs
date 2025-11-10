using IgnaCheck.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace IgnaCheck.Infrastructure.Data.Configurations;

/// <summary>
/// Entity Framework configuration for the Organization entity.
/// </summary>
public class OrganizationConfiguration : IEntityTypeConfiguration<Organization>
{
    public void Configure(EntityTypeBuilder<Organization> builder)
    {
        builder.ToTable("Organizations");

        builder.HasKey(e => e.Id);

        // Properties
        builder.Property(e => e.Name)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(e => e.Slug)
            .HasMaxLength(100);

        builder.Property(e => e.Domain)
            .HasMaxLength(200);

        builder.Property(e => e.LogoUrl)
            .HasMaxLength(500);

        builder.Property(e => e.Description)
            .HasMaxLength(1000);

        builder.Property(e => e.Industry)
            .HasMaxLength(100);

        builder.Property(e => e.CompanySize)
            .HasMaxLength(50);

        builder.Property(e => e.ContactEmail)
            .HasMaxLength(256);

        builder.Property(e => e.ContactPhone)
            .HasMaxLength(50);

        builder.Property(e => e.SubscriptionTier)
            .HasMaxLength(50);

        builder.Property(e => e.BillingEmail)
            .HasMaxLength(256);

        builder.Property(e => e.BillingCustomerId)
            .HasMaxLength(200);

        builder.Property(e => e.DeactivationReason)
            .HasMaxLength(500);

        builder.Property(e => e.CreatedBy)
            .HasMaxLength(450); // Standard ASP.NET Identity user ID length

        builder.Property(e => e.Settings)
            .HasColumnType("jsonb"); // Use jsonb for PostgreSQL, will work as nvarchar(max) in SQL Server

        // Indexes for performance
        builder.HasIndex(e => e.Slug)
            .IsUnique()
            .HasFilter("\"Slug\" IS NOT NULL") // Partial index for non-null slugs
            .HasDatabaseName("IX_Organizations_Slug");

        builder.HasIndex(e => e.Domain)
            .HasDatabaseName("IX_Organizations_Domain");

        builder.HasIndex(e => e.IsActive)
            .HasDatabaseName("IX_Organizations_IsActive");

        builder.HasIndex(e => e.CreatedBy)
            .HasDatabaseName("IX_Organizations_CreatedBy");

        builder.HasIndex(e => e.BillingCustomerId)
            .HasDatabaseName("IX_Organizations_BillingCustomerId");

        // Computed columns (ignored in EF Core, computed at runtime)
        builder.Ignore(e => e.IsTrialActive);
    }
}
