namespace IgnaCheck.Domain.Entities;

/// <summary>
/// Represents a user's membership in an organization (workspace).
/// This is the join entity that connects users to organizations with specific roles.
/// Supports the multi-tenant SaaS model where users can belong to multiple workspaces.
/// </summary>
public class OrganizationMember : BaseAuditableEntity
{
    /// <summary>
    /// The organization (workspace) this membership belongs to.
    /// </summary>
    public Guid OrganizationId { get; set; }

    /// <summary>
    /// The user's ID (maps to ApplicationUser.Id from Identity).
    /// Using string because IdentityUser uses string IDs by default.
    /// </summary>
    public string UserId { get; set; } = string.Empty;

    /// <summary>
    /// The role this user has within the organization.
    /// Examples: "Owner", "Admin", "Member", "Viewer".
    /// See WorkspaceRoles constants.
    /// </summary>
    public string Role { get; set; } = string.Empty;

    /// <summary>
    /// Date when the user joined the organization.
    /// </summary>
    public DateTime JoinedDate { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// User who invited this member (by email).
    /// Null if the user is the organization creator.
    /// </summary>
    public string? InvitedBy { get; set; }

    /// <summary>
    /// The invitation that led to this membership.
    /// Null if the user created the organization.
    /// </summary>
    public Guid? InvitationId { get; set; }

    /// <summary>
    /// Indicates whether this user is active in the organization.
    /// Inactive members cannot access the workspace.
    /// </summary>
    public bool IsActive { get; set; } = true;

    /// <summary>
    /// Date when the membership was deactivated.
    /// </summary>
    public DateTime? DeactivatedDate { get; set; }

    /// <summary>
    /// User who deactivated this membership.
    /// </summary>
    public string? DeactivatedBy { get; set; }

    /// <summary>
    /// Reason for deactivation (optional).
    /// </summary>
    public string? DeactivationReason { get; set; }

    /// <summary>
    /// Custom permissions or access overrides for this member (stored as JSON).
    /// Example: {"canDeleteProjects": false, "canInviteUsers": true}
    /// Allows granular permission control beyond role-based access.
    /// </summary>
    public string? CustomPermissions { get; set; }

    /// <summary>
    /// Date when the user last accessed this workspace.
    /// </summary>
    public DateTime? LastAccessedDate { get; set; }

    /// <summary>
    /// Notes about this membership (visible to admins only).
    /// </summary>
    public string? Notes { get; set; }

    // Navigation properties

    /// <summary>
    /// The organization this membership belongs to.
    /// </summary>
    public Organization Organization { get; set; } = null!;

    /// <summary>
    /// The invitation that led to this membership (if any).
    /// </summary>
    public Invitation? Invitation { get; set; }

    // Note: We don't add a navigation property to ApplicationUser here
    // because ApplicationUser is in the Infrastructure layer (Identity),
    // and Domain should not reference Infrastructure.
    // The relationship will be managed through the UserId string.
}
