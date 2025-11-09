namespace IgnaCheck.Domain.Constants;

/// <summary>
/// Defines role constants for the application.
/// Includes both system-level roles and workspace-level roles.
/// </summary>
public abstract class Roles
{
    /// <summary>
    /// System-level administrator with full platform access.
    /// Can manage all organizations, users, and system settings.
    /// </summary>
    public const string Administrator = nameof(Administrator);
}

/// <summary>
/// Defines workspace (organization) role constants.
/// These roles determine what users can do within a specific workspace.
/// </summary>
public abstract class WorkspaceRoles
{
    /// <summary>
    /// Workspace owner - full control over the workspace.
    /// Can delete the workspace, manage billing, and assign roles.
    /// Typically the user who created the workspace.
    /// </summary>
    public const string Owner = nameof(Owner);

    /// <summary>
    /// Workspace administrator - can manage most workspace settings.
    /// Can invite/remove users, manage projects, and configure settings.
    /// Cannot delete the workspace or change ownership.
    /// </summary>
    public const string Admin = nameof(Admin);

    /// <summary>
    /// Workspace member - standard user access.
    /// Can create and manage projects, upload documents, and collaborate.
    /// Cannot manage users or workspace settings.
    /// </summary>
    public const string Member = nameof(Member);

    /// <summary>
    /// Workspace viewer - read-only access.
    /// Can view projects, documents, and findings but cannot make changes.
    /// Useful for auditors, stakeholders, or observers.
    /// </summary>
    public const string Viewer = nameof(Viewer);

    /// <summary>
    /// All workspace roles in hierarchical order (most to least privileged).
    /// </summary>
    public static readonly string[] All = { Owner, Admin, Member, Viewer };

    /// <summary>
    /// Checks if a given role is valid.
    /// </summary>
    public static bool IsValid(string role) => All.Contains(role);

    /// <summary>
    /// Gets the hierarchy level of a role (lower number = more privileged).
    /// Owner = 1, Admin = 2, Member = 3, Viewer = 4, Unknown = 999
    /// </summary>
    public static int GetHierarchyLevel(string role) => role switch
    {
        Owner => 1,
        Admin => 2,
        Member => 3,
        Viewer => 4,
        _ => 999
    };

    /// <summary>
    /// Checks if role1 has equal or higher privilege than role2.
    /// </summary>
    public static bool HasEqualOrHigherPrivilege(string role1, string role2)
    {
        return GetHierarchyLevel(role1) <= GetHierarchyLevel(role2);
    }
}