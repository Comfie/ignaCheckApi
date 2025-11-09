namespace IgnaCheck.Domain.Entities;

/// <summary>
/// Represents a comment on a remediation task.
/// Supports threaded discussions and mentions.
/// </summary>
public class TaskComment : BaseAuditableEntity
{
    /// <summary>
    /// The task this comment belongs to.
    /// </summary>
    public Guid TaskId { get; set; }

    /// <summary>
    /// Parent comment ID for threaded replies.
    /// </summary>
    public Guid? ParentCommentId { get; set; }

    /// <summary>
    /// The comment text content.
    /// Supports markdown.
    /// </summary>
    public string Content { get; set; } = string.Empty;

    /// <summary>
    /// User mentions in the comment.
    /// Stored as JSON array of user IDs.
    /// </summary>
    public string? Mentions { get; set; }

    /// <summary>
    /// Indicates whether this comment has been edited.
    /// </summary>
    public bool IsEdited { get; set; }

    /// <summary>
    /// Date when the comment was last edited.
    /// </summary>
    public DateTime? EditedDate { get; set; }

    // Navigation properties

    /// <summary>
    /// The task this comment belongs to.
    /// </summary>
    public RemediationTask Task { get; set; } = null!;

    /// <summary>
    /// Parent comment for threaded replies.
    /// </summary>
    public TaskComment? ParentComment { get; set; }

    /// <summary>
    /// Replies to this comment.
    /// </summary>
    public ICollection<TaskComment> Replies { get; set; } = new List<TaskComment>();
}
