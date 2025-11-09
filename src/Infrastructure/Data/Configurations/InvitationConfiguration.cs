using IgnaCheck.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace IgnaCheck.Infrastructure.Data.Configurations;

/// <summary>
/// Entity Framework configuration for the Invitation entity.
/// </summary>
public class InvitationConfiguration : IEntityTypeConfiguration<Invitation>
{
    public void Configure(EntityTypeBuilder<Invitation> builder)
    {
        builder.ToTable("Invitations");

        builder.HasKey(e => e.Id);

        // Properties
        builder.Property(e => e.Email)
            .IsRequired()
            .HasMaxLength(256);

        builder.Property(e => e.FirstName)
            .HasMaxLength(100);

        builder.Property(e => e.LastName)
            .HasMaxLength(100);

        builder.Property(e => e.Role)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(e => e.InvitedBy)
            .IsRequired()
            .HasMaxLength(450); // Standard ASP.NET Identity user ID length

        builder.Property(e => e.Token)
            .IsRequired()
            .HasMaxLength(256);

        builder.Property(e => e.AcceptedBy)
            .HasMaxLength(450);

        builder.Property(e => e.DeclineReason)
            .HasMaxLength(500);

        builder.Property(e => e.RevokedBy)
            .HasMaxLength(450);

        builder.Property(e => e.RevokeReason)
            .HasMaxLength(500);

        builder.Property(e => e.Message)
            .HasMaxLength(1000);

        // Indexes
        builder.HasIndex(e => e.OrganizationId)
            .HasDatabaseName("IX_Invitations_OrganizationId");

        builder.HasIndex(e => e.Email)
            .HasDatabaseName("IX_Invitations_Email");

        // Composite index for faster lookups of pending invitations by email
        builder.HasIndex(e => new { e.OrganizationId, e.Email, e.Status })
            .HasDatabaseName("IX_Invitations_OrganizationId_Email_Status");

        builder.HasIndex(e => e.Token)
            .IsUnique()
            .HasDatabaseName("IX_Invitations_Token");

        builder.HasIndex(e => e.Status)
            .HasDatabaseName("IX_Invitations_Status");

        builder.HasIndex(e => e.ExpiresDate)
            .HasDatabaseName("IX_Invitations_ExpiresDate");

        // Relationships
        builder.HasOne(e => e.Organization)
            .WithMany(o => o.Invitations)
            .HasForeignKey(e => e.OrganizationId)
            .OnDelete(DeleteBehavior.Cascade);

        // The OrganizationMember navigation is configured in OrganizationMemberConfiguration
    }
}
