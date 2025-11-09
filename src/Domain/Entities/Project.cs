namespace IgnaCheck.Domain.Entities;

/// <summary>
/// Represents an audit project/engagement within an organization.
/// This is the main aggregate for organizing compliance work.
/// </summary>
public class Project : BaseAuditableEntity, ITenantEntity
{
    public string Name { get; set; } = string.Empty;

    public string? Description { get; set; }

    public ProjectStatus Status { get; set; } = ProjectStatus.Draft;

    public DateTime? TargetDate { get; set; }

    // Foreign keys
    public Guid OrganizationId { get; set; }

    // Navigation properties
    public Organization Organization { get; set; } = null!;

    /// <summary>
    /// Compliance frameworks assigned to this project.
    /// </summary>
    public ICollection<ProjectFramework> ProjectFrameworks { get; set; } = new List<ProjectFramework>();

    /// <summary>
    /// Documents uploaded to this project.
    /// </summary>
    public ICollection<Document> Documents { get; set; } = new List<Document>();

    /// <summary>
    /// Compliance findings for this project.
    /// </summary>
    public ICollection<ComplianceFinding> Findings { get; set; } = new List<ComplianceFinding>();

    /// <summary>
    /// Remediation tasks for this project.
    /// </summary>
    public ICollection<RemediationTask> RemediationTasks { get; set; } = new List<RemediationTask>();

    /// <summary>
    /// Project members with their roles.
    /// </summary>
    public ICollection<ProjectMember> ProjectMembers { get; set; } = new List<ProjectMember>();
}
