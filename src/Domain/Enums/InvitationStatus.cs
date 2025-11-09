namespace IgnaCheck.Domain.Enums;

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
