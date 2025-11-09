using IgnaCheck.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace IgnaCheck.Infrastructure.Data.Configurations;

/// <summary>
/// Entity Framework configuration for the OrganizationMember entity.
/// </summary>
public class OrganizationMemberConfiguration : IEntityTypeConfiguration<OrganizationMember>
{
    public void Configure(EntityTypeBuilder<OrganizationMember> builder)
    {
        builder.ToTable("OrganizationMembers");

        builder.HasKey(e => e.Id);

        // Properties
        builder.Property(e => e.UserId)
            .IsRequired()
            .HasMaxLength(450); // Standard ASP.NET Identity user ID length

        builder.Property(e => e.Role)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(e => e.InvitedBy)
            .HasMaxLength(450);

        builder.Property(e => e.DeactivatedBy)
            .HasMaxLength(450);

        builder.Property(e => e.DeactivationReason)
            .HasMaxLength(500);

        builder.Property(e => e.CustomPermissions)
            .HasColumnType("jsonb"); // Use jsonb for PostgreSQL, will work as nvarchar(max) in SQL Server

        builder.Property(e => e.Notes)
            .HasMaxLength(2000);

        // Indexes
        builder.HasIndex(e => e.OrganizationId)
            .HasDatabaseName("IX_OrganizationMembers_OrganizationId");

        builder.HasIndex(e => e.UserId)
            .HasDatabaseName("IX_OrganizationMembers_UserId");

        // Composite unique index: One user can only be a member of an organization once
        builder.HasIndex(e => new { e.OrganizationId, e.UserId })
            .IsUnique()
            .HasDatabaseName("IX_OrganizationMembers_OrganizationId_UserId");

        builder.HasIndex(e => e.InvitationId)
            .HasDatabaseName("IX_OrganizationMembers_InvitationId");

        builder.HasIndex(e => e.IsActive)
            .HasDatabaseName("IX_OrganizationMembers_IsActive");

        // Relationships
        builder.HasOne(e => e.Organization)
            .WithMany(o => o.Members)
            .HasForeignKey(e => e.OrganizationId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(e => e.Invitation)
            .WithOne(i => i.OrganizationMember)
            .HasForeignKey<OrganizationMember>(e => e.InvitationId)
            .OnDelete(DeleteBehavior.SetNull);

        // Note: We don't configure the relationship to ApplicationUser here
        // because ApplicationUser is in the Infrastructure layer (Identity).
        // The UserId is a string foreign key that will be enforced at the application level.
    }
}
