using IgnaCheck.Application.Common.Interfaces;
using IgnaCheck.Domain.Enums;

namespace IgnaCheck.Application.Users.Commands.RevokeInvitation;

/// <summary>
/// Command to revoke a pending invitation.
/// </summary>
public record RevokeInvitationCommand : IRequest<Result>
{
    /// <summary>
    /// Invitation ID to revoke.
    /// </summary>
    public Guid InvitationId { get; init; }

    /// <summary>
    /// Workspace ID (optional - uses current workspace from JWT if not provided).
    /// </summary>
    public Guid? WorkspaceId { get; init; }
}

/// <summary>
/// Handler for the RevokeInvitationCommand.
/// </summary>
public class RevokeInvitationCommandHandler : IRequestHandler<RevokeInvitationCommand, Result>
{
    private readonly IApplicationDbContext _context;
    private readonly IUser _currentUser;

    public RevokeInvitationCommandHandler(
        IApplicationDbContext context,
        IUser currentUser)
    {
        _context = context;
        _currentUser = currentUser;
    }

    public async Task<Result> Handle(RevokeInvitationCommand request, CancellationToken cancellationToken)
    {
        // Ensure user is authenticated
        if (string.IsNullOrEmpty(_currentUser.Id))
        {
            return Result.Failure(new[] { "User must be authenticated." });
        }

        // Determine workspace ID
        var workspaceId = request.WorkspaceId ?? _currentUser.OrganizationId;
        if (!workspaceId.HasValue)
        {
            return Result.Failure(new[] { "Workspace ID is required." });
        }

        // Find invitation
        var invitation = await _context.Invitations
            .FirstOrDefaultAsync(i => i.Id == request.InvitationId && i.OrganizationId == workspaceId.Value, cancellationToken);

        if (invitation == null)
        {
            return Result.Failure(new[] { "Invitation not found." });
        }

        // Check if current user has permission to revoke (must be Owner or Admin)
        var currentUserMembership = await _context.OrganizationMembers
            .FirstOrDefaultAsync(m => m.OrganizationId == workspaceId.Value && m.UserId == _currentUser.Id && m.IsActive, cancellationToken);

        if (currentUserMembership == null)
        {
            return Result.Failure(new[] { "You are not a member of this workspace." });
        }

        if (currentUserMembership.Role != Domain.Constants.WorkspaceRoles.Owner && currentUserMembership.Role != Domain.Constants.WorkspaceRoles.Admin)
        {
            return Result.Failure(new[] { "Only workspace Owners and Admins can revoke invitations." });
        }

        // Check if invitation is still pending
        if (invitation.Status != InvitationStatus.Pending)
        {
            var statusMessage = invitation.Status switch
            {
                InvitationStatus.Accepted => "This invitation has already been accepted and cannot be revoked.",
                InvitationStatus.Declined => "This invitation has already been declined.",
                InvitationStatus.Revoked => "This invitation has already been revoked.",
                InvitationStatus.Expired => "This invitation has already expired.",
                _ => "This invitation cannot be revoked."
            };
            return Result.Failure(new[] { statusMessage });
        }

        // Revoke invitation
        invitation.Status = InvitationStatus.Revoked;

        await _context.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
