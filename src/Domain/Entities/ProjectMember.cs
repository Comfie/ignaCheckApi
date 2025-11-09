namespace IgnaCheck.Domain.Entities;

/// <summary>
/// Represents a user's membership and role within a specific project.
/// Provides project-level access control in addition to workspace-level roles.
/// </summary>
public class ProjectMember : BaseAuditableEntity, ITenantEntity
{
    /// <summary>
    /// Organization (tenant) that owns this project membership.
    /// </summary>
    public Guid OrganizationId { get; set; }

    /// <summary>
    /// Project ID.
    /// </summary>
    public Guid ProjectId { get; set; }

    /// <summary>
    /// User ID (from Identity system).
    /// </summary>
    public string UserId { get; set; } = string.Empty;

    /// <summary>
    /// User's display name (denormalized for performance).
    /// </summary>
    public string UserName { get; set; } = string.Empty;

    /// <summary>
    /// User's email (denormalized for performance).
    /// </summary>
    public string UserEmail { get; set; } = string.Empty;

    /// <summary>
    /// User's role within this specific project.
    /// </summary>
    public ProjectRole Role { get; set; } = ProjectRole.Viewer;

    /// <summary>
    /// Date when the user was added to the project.
    /// </summary>
    public DateTime JoinedDate { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// User who added this member to the project.
    /// </summary>
    public string? AddedBy { get; set; }

    /// <summary>
    /// Indicates whether the member is active in this project.
    /// </summary>
    public bool IsActive { get; set; } = true;

    /// <summary>
    /// Date when the member was removed from the project.
    /// </summary>
    public DateTime? RemovedDate { get; set; }

    /// <summary>
    /// User who removed this member from the project.
    /// </summary>
    public string? RemovedBy { get; set; }

    // Navigation properties

    /// <summary>
    /// The organization this project membership belongs to.
    /// </summary>
    public Organization Organization { get; set; } = null!;

    /// <summary>
    /// The project this membership is for.
    /// </summary>
    public Project Project { get; set; } = null!;
}

/// <summary>
/// Project-level roles for fine-grained access control.
/// These are separate from workspace-level roles (Owner, Admin, Member, Viewer).
/// </summary>
public enum ProjectRole
{
    /// <summary>
    /// Project owner - Full control over the project, can delete, archive, manage members.
    /// </summary>
    Owner = 0,

    /// <summary>
    /// Project contributor - Can upload documents, create findings, assign tasks.
    /// </summary>
    Contributor = 1,

    /// <summary>
    /// Project viewer - Read-only access to project data.
    /// </summary>
    Viewer = 2
}
