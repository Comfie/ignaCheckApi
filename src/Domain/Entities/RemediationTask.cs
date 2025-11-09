namespace IgnaCheck.Domain.Entities;

/// <summary>
/// Represents a remediation task created to address a compliance finding.
/// Tasks can be assigned to users, tracked, and marked as complete.
/// </summary>
public class RemediationTask : BaseAuditableEntity, ITenantEntity
{
    /// <summary>
    /// Organization (tenant) that owns this task.
    /// </summary>
    public Guid OrganizationId { get; set; }

    /// <summary>
    /// The project this task belongs to.
    /// </summary>
    public Guid ProjectId { get; set; }

    /// <summary>
    /// The compliance finding this task addresses.
    /// </summary>
    public Guid? FindingId { get; set; }

    /// <summary>
    /// Task title.
    /// </summary>
    public string Title { get; set; } = string.Empty;

    /// <summary>
    /// Detailed task description.
    /// </summary>
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// Current status of the task.
    /// </summary>
    public TaskStatus Status { get; set; } = TaskStatus.Open;

    /// <summary>
    /// Priority level of the task.
    /// </summary>
    public PriorityLevel Priority { get; set; } = PriorityLevel.Medium;

    /// <summary>
    /// User assigned to complete this task.
    /// </summary>
    public string? AssignedTo { get; set; }

    /// <summary>
    /// Date when the task was assigned.
    /// </summary>
    public DateTime? AssignedDate { get; set; }

    /// <summary>
    /// Due date for task completion.
    /// </summary>
    public DateTime? DueDate { get; set; }

    /// <summary>
    /// Date when the task was started.
    /// </summary>
    public DateTime? StartedDate { get; set; }

    /// <summary>
    /// Date when the task was completed.
    /// </summary>
    public DateTime? CompletedDate { get; set; }

    /// <summary>
    /// User who completed the task.
    /// </summary>
    public string? CompletedBy { get; set; }

    /// <summary>
    /// Estimated effort in hours.
    /// </summary>
    public decimal? EstimatedHours { get; set; }

    /// <summary>
    /// Actual effort in hours.
    /// </summary>
    public decimal? ActualHours { get; set; }

    /// <summary>
    /// Completion percentage (0-100).
    /// </summary>
    public int PercentComplete { get; set; }

    /// <summary>
    /// Tags for categorization and filtering.
    /// Stored as JSON array.
    /// </summary>
    public string? Tags { get; set; }

    /// <summary>
    /// Additional notes or comments.
    /// </summary>
    public string? Notes { get; set; }

    // Navigation properties

    /// <summary>
    /// The organization this task belongs to.
    /// </summary>
    public Organization Organization { get; set; } = null!;

    /// <summary>
    /// The project this task belongs to.
    /// </summary>
    public Project Project { get; set; } = null!;

    /// <summary>
    /// The compliance finding this task addresses (optional).
    /// </summary>
    public ComplianceFinding? Finding { get; set; }

    /// <summary>
    /// Comments on this task.
    /// </summary>
    public ICollection<TaskComment> Comments { get; set; } = new List<TaskComment>();

    /// <summary>
    /// Attachments/evidence for this task.
    /// </summary>
    public ICollection<TaskAttachment> Attachments { get; set; } = new List<TaskAttachment>();
}

/// <summary>
/// Status of a remediation task.
/// </summary>
public enum TaskStatus
{
    Open = 0,
    InProgress = 1,
    UnderReview = 2,
    Blocked = 3,
    Completed = 4,
    Cancelled = 5
}

/// <summary>
/// Priority level of a remediation task.
/// </summary>
public enum PriorityLevel
{
    Low = 0,
    Medium = 1,
    High = 2,
    Critical = 3
}
