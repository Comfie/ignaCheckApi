namespace IgnaCheck.Domain.Entities;

/// <summary>
/// Represents an activity log entry for audit trail purposes.
/// Tracks all significant actions performed by users within the system.
/// </summary>
public class ActivityLog : BaseEntity, ITenantEntity
{
    /// <summary>
    /// Organization (tenant) that this activity belongs to.
    /// </summary>
    public Guid OrganizationId { get; set; }

    /// <summary>
    /// Project this activity is related to (null for workspace-level activities).
    /// </summary>
    public Guid? ProjectId { get; set; }

    /// <summary>
    /// User who performed the action.
    /// </summary>
    public string UserId { get; set; } = string.Empty;

    /// <summary>
    /// Display name of the user who performed the action (denormalized for performance).
    /// </summary>
    public string UserName { get; set; } = string.Empty;

    /// <summary>
    /// Email of the user who performed the action (denormalized for performance).
    /// </summary>
    public string UserEmail { get; set; } = string.Empty;

    /// <summary>
    /// Type of activity performed.
    /// </summary>
    public ActivityType ActivityType { get; set; }

    /// <summary>
    /// Entity type that was acted upon (e.g., "Project", "Document", "User").
    /// </summary>
    public string EntityType { get; set; } = string.Empty;

    /// <summary>
    /// ID of the entity that was acted upon.
    /// </summary>
    public Guid? EntityId { get; set; }

    /// <summary>
    /// Name or title of the entity (denormalized for performance).
    /// </summary>
    public string? EntityName { get; set; }

    /// <summary>
    /// Description of the action performed.
    /// Example: "Created project 'Q2 2025 SOC 2 Audit'"
    /// </summary>
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// Additional details or metadata about the activity (stored as JSON).
    /// Example: {"oldValue": "Draft", "newValue": "Active"}
    /// </summary>
    public string? Metadata { get; set; }

    /// <summary>
    /// Timestamp when the activity occurred.
    /// </summary>
    public DateTime OccurredAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// IP address of the user who performed the action.
    /// </summary>
    public string? IpAddress { get; set; }

    /// <summary>
    /// User agent string (browser/client information).
    /// </summary>
    public string? UserAgent { get; set; }

    // Navigation properties

    /// <summary>
    /// The organization this activity belongs to.
    /// </summary>
    public Organization Organization { get; set; } = null!;

    /// <summary>
    /// The project this activity is related to (if applicable).
    /// </summary>
    public Project? Project { get; set; }
}
