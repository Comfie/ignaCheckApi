namespace IgnaCheck.Domain.Enums;

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
