namespace IgnaCheck.Domain.Entities;

/// <summary>
/// Represents a notification sent to a user.
/// Used for in-app notification tracking and audit trail.
/// </summary>
public class Notification : BaseAuditableEntity
{
    /// <summary>
    /// User ID who receives the notification.
    /// </summary>
    public string UserId { get; set; } = string.Empty;

    /// <summary>
    /// Type of notification.
    /// </summary>
    public NotificationType Type { get; set; }

    /// <summary>
    /// Notification title/subject.
    /// </summary>
    public string Title { get; set; } = string.Empty;

    /// <summary>
    /// Notification message body.
    /// </summary>
    public string Message { get; set; } = string.Empty;

    /// <summary>
    /// Link/URL related to the notification (optional).
    /// </summary>
    public string? Link { get; set; }

    /// <summary>
    /// Related entity ID (e.g., FindingId, ProjectId, etc.).
    /// </summary>
    public Guid? RelatedEntityId { get; set; }

    /// <summary>
    /// Related entity type (e.g., "Finding", "Project", "Task").
    /// </summary>
    public string? RelatedEntityType { get; set; }

    /// <summary>
    /// Indicates whether the notification has been read.
    /// </summary>
    public bool IsRead { get; set; }

    /// <summary>
    /// Date when the notification was read.
    /// </summary>
    public DateTime? ReadDate { get; set; }

    /// <summary>
    /// Indicates whether an email was sent.
    /// </summary>
    public bool EmailSent { get; set; }

    /// <summary>
    /// Date when the email was sent.
    /// </summary>
    public DateTime? EmailSentDate { get; set; }

    /// <summary>
    /// Error message if email sending failed.
    /// </summary>
    public string? EmailError { get; set; }
}
