namespace IgnaCheck.Domain.Entities;

/// <summary>
/// Represents a comment on a compliance finding.
/// Supports threaded discussions and mentions for collaboration on remediation.
/// </summary>
public class FindingComment : BaseAuditableEntity
{
    /// <summary>
    /// The finding this comment belongs to.
    /// </summary>
    public Guid FindingId { get; set; }

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

    /// <summary>
    /// Indicates whether this comment marks the finding as resolved.
    /// </summary>
    public bool IsResolutionComment { get; set; }

    // Navigation properties

    /// <summary>
    /// The finding this comment belongs to.
    /// </summary>
    public ComplianceFinding Finding { get; set; } = null!;

    /// <summary>
    /// Parent comment for threaded replies.
    /// </summary>
    public FindingComment? ParentComment { get; set; }

    /// <summary>
    /// Replies to this comment.
    /// </summary>
    public ICollection<FindingComment> Replies { get; set; } = new List<FindingComment>();
}
