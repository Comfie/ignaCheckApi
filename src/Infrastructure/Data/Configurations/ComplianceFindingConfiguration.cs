using IgnaCheck.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace IgnaCheck.Infrastructure.Data.Configurations;

public class ComplianceFindingConfiguration : IEntityTypeConfiguration<ComplianceFinding>
{
    public void Configure(EntityTypeBuilder<ComplianceFinding> builder)
    {
        builder.Property(t => t.FindingCode)
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(t => t.Title)
            .HasMaxLength(500)
            .IsRequired();

        builder.Property(t => t.Description)
            .HasMaxLength(4000)
            .IsRequired();

        // Relationship with Organization
        builder.HasOne(f => f.Organization)
            .WithMany()
            .HasForeignKey(f => f.OrganizationId)
            .OnDelete(DeleteBehavior.Cascade);

        // Relationship with Project
        builder.HasOne(f => f.Project)
            .WithMany()
            .HasForeignKey(f => f.ProjectId)
            .OnDelete(DeleteBehavior.Cascade);

        // Relationship with ComplianceControl
        builder.HasOne(f => f.Control)
            .WithMany()
            .HasForeignKey(f => f.ControlId)
            .OnDelete(DeleteBehavior.Restrict);

        // Configure the one-to-one relationship with RemediationTask
        // RemediationTask is the dependent side (it has the FK)
        // ComplianceFinding.RemediationTaskId is just a denormalized field, not the FK for the relationship
        builder.HasOne(f => f.RemediationTask)
            .WithOne(t => t.Finding)
            .HasForeignKey<RemediationTask>(t => t.FindingId)
            .OnDelete(DeleteBehavior.SetNull);

        // RemediationTaskId is a regular property, not a foreign key in the relationship
        builder.Property(f => f.RemediationTaskId)
            .IsRequired(false);

        // Indexes for common queries
        builder.HasIndex(t => t.OrganizationId);
        builder.HasIndex(t => t.ProjectId);
        builder.HasIndex(t => t.ControlId);
        builder.HasIndex(t => t.Status);
        builder.HasIndex(t => t.WorkflowStatus);
        builder.HasIndex(t => t.FindingCode);
    }
}
