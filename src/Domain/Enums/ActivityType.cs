namespace IgnaCheck.Domain.Enums;

/// <summary>
/// Types of activities that can be logged in the system.
/// </summary>
public enum ActivityType
{
    // Workspace/Organization activities
    WorkspaceCreated = 1,
    WorkspaceUpdated = 2,
    WorkspaceDeleted = 3,
    WorkspaceSettingsChanged = 4,

    // User/Member activities
    UserInvited = 10,
    UserJoined = 11,
    UserRemoved = 12,
    UserRoleChanged = 13,
    UserProfileUpdated = 14,

    // Project activities
    ProjectCreated = 20,
    ProjectUpdated = 21,
    ProjectDeleted = 22,
    ProjectArchived = 23,
    ProjectRestored = 24,
    ProjectMemberAdded = 25,
    ProjectMemberRemoved = 26,
    ProjectMemberRoleChanged = 27,

    // Document activities
    DocumentUploaded = 30,
    DocumentUpdated = 31,
    DocumentDeleted = 32,
    DocumentDownloaded = 33,
    DocumentVersionUploaded = 34,

    // Framework activities
    FrameworkAdded = 40,
    FrameworkRemoved = 41,

    // Compliance check activities
    ComplianceCheckStarted = 50,
    ComplianceCheckCompleted = 51,
    ComplianceCheckFailed = 52,

    // Finding activities
    FindingCreated = 60,
    FindingUpdated = 61,
    FindingStatusChanged = 62,
    FindingAssigned = 63,
    FindingCommentAdded = 64,

    // Task activities
    TaskCreated = 70,
    TaskUpdated = 71,
    TaskCompleted = 72,
    TaskAssigned = 73,
    TaskCommentAdded = 74,
    TaskAttachmentAdded = 75,

    // Authentication activities
    UserLoggedIn = 80,
    UserLoggedOut = 81,
    PasswordChanged = 82,
    PasswordReset = 83,
    EmailVerified = 84,

    // Report activities
    ReportGenerated = 90,
    ReportExported = 91,

    // Other
    Other = 999
}
