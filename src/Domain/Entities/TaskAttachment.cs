namespace IgnaCheck.Domain.Entities;

/// <summary>
/// Represents a file attachment on a remediation task.
/// Links tasks to evidence documents.
/// </summary>
public class TaskAttachment : BaseAuditableEntity
{
    /// <summary>
    /// The task this attachment belongs to.
    /// </summary>
    public Guid TaskId { get; set; }

    /// <summary>
    /// The document that is attached.
    /// </summary>
    public Guid DocumentId { get; set; }

    /// <summary>
    /// Description or purpose of this attachment.
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// Type of attachment (e.g., "Evidence", "Screenshot", "Policy").
    /// </summary>
    public string? AttachmentType { get; set; }

    // Navigation properties

    /// <summary>
    /// The task this attachment belongs to.
    /// </summary>
    public RemediationTask Task { get; set; } = null!;

    /// <summary>
    /// The attached document.
    /// </summary>
    public Document Document { get; set; } = null!;
}
