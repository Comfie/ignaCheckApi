using IgnaCheck.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace IgnaCheck.Infrastructure.Data.Configurations;

public class RemediationTaskConfiguration : IEntityTypeConfiguration<RemediationTask>
{
    public void Configure(EntityTypeBuilder<RemediationTask> builder)
    {
        builder.Property(t => t.Title)
            .HasMaxLength(500)
            .IsRequired();

        builder.Property(t => t.Description)
            .HasMaxLength(4000)
            .IsRequired();

        // Relationship with Organization
        builder.HasOne(t => t.Organization)
            .WithMany()
            .HasForeignKey(t => t.OrganizationId)
            .OnDelete(DeleteBehavior.Cascade);

        // Relationship with Project
        builder.HasOne(t => t.Project)
            .WithMany()
            .HasForeignKey(t => t.ProjectId)
            .OnDelete(DeleteBehavior.Cascade);

        // The relationship with ComplianceFinding is configured in ComplianceFindingConfiguration
        // This side (RemediationTask) is the dependent side with the foreign key

        // Indexes for common queries
        builder.HasIndex(t => t.OrganizationId);
        builder.HasIndex(t => t.ProjectId);
        builder.HasIndex(t => t.FindingId);
        builder.HasIndex(t => t.Status);
        builder.HasIndex(t => t.AssignedTo);
    }
}
