namespace IgnaCheck.Domain.Enums;

/// <summary>
/// Organization (workspace) level roles for access control.
/// </summary>
public enum OrganizationRole
{
    /// <summary>
    /// Workspace owner - Full control, can delete workspace, manage billing.
    /// </summary>
    Owner = 0,

    /// <summary>
    /// Workspace admin - Can manage members, projects, and settings.
    /// </summary>
    Admin = 1,

    /// <summary>
    /// Workspace member - Can create and manage own projects.
    /// </summary>
    Member = 2,

    /// <summary>
    /// Workspace viewer - Read-only access across all projects.
    /// </summary>
    Viewer = 3
}
