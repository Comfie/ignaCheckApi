using IgnaCheck.Domain.Entities;

namespace IgnaCheck.Application.Common.Interfaces;

public interface IApplicationDbContext
{
    DbSet<Organization> Organizations { get; }

    DbSet<Project> Projects { get; }

    DbSet<OrganizationMember> OrganizationMembers { get; }

    DbSet<Invitation> Invitations { get; }

    DbSet<ActivityLog> ActivityLogs { get; }

    DbSet<ProjectMember> ProjectMembers { get; }

    DbSet<Document> Documents { get; }

    DbSet<ComplianceFramework> ComplianceFrameworks { get; }

    DbSet<ComplianceControl> ComplianceControls { get; }

    DbSet<ProjectFramework> ProjectFrameworks { get; }

    DbSet<ComplianceFinding> ComplianceFindings { get; }

    DbSet<FindingEvidence> FindingEvidence { get; }

    DbSet<RemediationTask> RemediationTasks { get; }

    DbSet<TaskComment> TaskComments { get; }

    DbSet<TaskAttachment> TaskAttachments { get; }

    DbSet<FindingComment> FindingComments { get; }

    DbSet<Notification> Notifications { get; }

    DbSet<NotificationPreference> NotificationPreferences { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken);
}
