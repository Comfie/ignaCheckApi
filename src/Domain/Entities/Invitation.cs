namespace IgnaCheck.Domain.Entities;

/// <summary>
/// Represents an invitation sent to a user to join an organization.
/// Invitations have a limited validity period and can be accepted, declined, or expire.
/// </summary>
public class Invitation : BaseAuditableEntity
{
    /// <summary>
    /// The organization this invitation is for.
    /// </summary>
    public Guid OrganizationId { get; set; }

    /// <summary>
    /// Email address of the invited user.
    /// </summary>
    public string Email { get; set; } = string.Empty;

    /// <summary>
    /// First name of the invited user (optional, for personalization).
    /// </summary>
    public string? FirstName { get; set; }

    /// <summary>
    /// Last name of the invited user (optional, for personalization).
    /// </summary>
    public string? LastName { get; set; }

    /// <summary>
    /// The role the user will have once they accept the invitation.
    /// Examples: "Admin", "Member", "Viewer".
    /// See WorkspaceRoles constants.
    /// </summary>
    public string Role { get; set; } = string.Empty;

    /// <summary>
    /// User who sent the invitation (by ID).
    /// </summary>
    public string InvitedBy { get; set; } = string.Empty;

    /// <summary>
    /// Date when the invitation was sent.
    /// </summary>
    public DateTime InvitedDate { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Unique token used to accept the invitation.
    /// Should be cryptographically secure and unguessable.
    /// </summary>
    public string Token { get; set; } = string.Empty;

    /// <summary>
    /// Date when the invitation expires.
    /// Default: 7 days from creation.
    /// </summary>
    public DateTime ExpiresDate { get; set; } = DateTime.UtcNow.AddDays(7);

    /// <summary>
    /// Current status of the invitation.
    /// </summary>
    public InvitationStatus Status { get; set; } = InvitationStatus.Pending;

    /// <summary>
    /// Date when the invitation was accepted.
    /// </summary>
    public DateTime? AcceptedDate { get; set; }

    /// <summary>
    /// User who accepted the invitation (by ID).
    /// Populated when the invitation is accepted.
    /// </summary>
    public string? AcceptedBy { get; set; }

    /// <summary>
    /// Date when the invitation was declined.
    /// </summary>
    public DateTime? DeclinedDate { get; set; }

    /// <summary>
    /// Reason provided for declining the invitation (optional).
    /// </summary>
    public string? DeclineReason { get; set; }

    /// <summary>
    /// Date when the invitation was revoked/cancelled.
    /// </summary>
    public DateTime? RevokedDate { get; set; }

    /// <summary>
    /// User who revoked the invitation (by ID).
    /// </summary>
    public string? RevokedBy { get; set; }

    /// <summary>
    /// Reason for revoking the invitation (optional).
    /// </summary>
    public string? RevokeReason { get; set; }

    /// <summary>
    /// Personal message to include in the invitation email (optional).
    /// </summary>
    public string? Message { get; set; }

    /// <summary>
    /// Number of times the invitation email was sent.
    /// Incremented on resend.
    /// </summary>
    public int SendCount { get; set; } = 1;

    /// <summary>
    /// Date when the invitation was last sent/resent.
    /// </summary>
    public DateTime LastSentDate { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Indicates whether this invitation has expired.
    /// </summary>
    public bool IsExpired => Status == InvitationStatus.Pending && DateTime.UtcNow > ExpiresDate;

    /// <summary>
    /// Indicates whether this invitation can still be accepted.
    /// </summary>
    public bool CanBeAccepted => Status == InvitationStatus.Pending && !IsExpired;

    // Navigation properties

    /// <summary>
    /// The organization this invitation is for.
    /// </summary>
    public Organization Organization { get; set; } = null!;

    /// <summary>
    /// The organization membership created from this invitation (if accepted).
    /// </summary>
    public OrganizationMember? OrganizationMember { get; set; }
}

/// <summary>
/// Status of an invitation.
/// </summary>
public enum InvitationStatus
{
    /// <summary>
    /// Invitation has been sent and is awaiting response.
    /// </summary>
    Pending = 0,

    /// <summary>
    /// Invitation has been accepted.
    /// </summary>
    Accepted = 1,

    /// <summary>
    /// Invitation has been declined by the invitee.
    /// </summary>
    Declined = 2,

    /// <summary>
    /// Invitation has been revoked/cancelled by an admin.
    /// </summary>
    Revoked = 3,

    /// <summary>
    /// Invitation has expired without being accepted.
    /// </summary>
    Expired = 4
}
