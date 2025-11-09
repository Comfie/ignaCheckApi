namespace IgnaCheck.Domain.Enums;

/// <summary>
/// Types of notifications that can be sent to users.
/// </summary>
public enum NotificationType
{
    /// <summary>
    /// User was invited to join a workspace.
    /// </summary>
    WorkspaceInvitation = 0,

    /// <summary>
    /// User was added to a project.
    /// </summary>
    ProjectMemberAdded = 1,

    /// <summary>
    /// Finding was assigned to user.
    /// </summary>
    FindingAssigned = 2,

    /// <summary>
    /// User was mentioned in a finding comment.
    /// </summary>
    FindingCommentMention = 3,

    /// <summary>
    /// User was mentioned in a task comment.
    /// </summary>
    TaskCommentMention = 4,

    /// <summary>
    /// Task was assigned to user.
    /// </summary>
    TaskAssigned = 5,

    /// <summary>
    /// Compliance audit check completed.
    /// </summary>
    AuditCheckCompleted = 6,

    /// <summary>
    /// Finding is approaching due date.
    /// </summary>
    FindingDueSoon = 7,

    /// <summary>
    /// Task is approaching due date.
    /// </summary>
    TaskDueSoon = 8,

    /// <summary>
    /// New critical finding discovered.
    /// </summary>
    CriticalFindingDetected = 9,

    /// <summary>
    /// Project compliance score changed significantly.
    /// </summary>
    ComplianceScoreChanged = 10
}
