using IgnaCheck.Application.Common.Interfaces;
using IgnaCheck.Domain.Enums;

namespace IgnaCheck.Application.Users.Commands.DeclineInvitation;

/// <summary>
/// Command to decline a workspace invitation.
/// </summary>
public record DeclineInvitationCommand : IRequest<Result>
{
    /// <summary>
    /// Invitation token.
    /// </summary>
    public string Token { get; init; } = string.Empty;
}

/// <summary>
/// Handler for the DeclineInvitationCommand.
/// </summary>
public class DeclineInvitationCommandHandler : IRequestHandler<DeclineInvitationCommand, Result>
{
    private readonly IApplicationDbContext _context;
    private readonly IUser _currentUser;
    private readonly IIdentityService _identityService;

    public DeclineInvitationCommandHandler(
        IApplicationDbContext context,
        IUser currentUser,
        IIdentityService identityService)
    {
        _context = context;
        _currentUser = currentUser;
        _identityService = identityService;
    }

    public async Task<Result> Handle(DeclineInvitationCommand request, CancellationToken cancellationToken)
    {
        // Ensure user is authenticated
        if (string.IsNullOrEmpty(_currentUser.Id))
        {
            return Result.Failure(new[] { "User must be authenticated to decline an invitation." });
        }

        // Get user details
        var user = await _identityService.GetUserByIdAsync(_currentUser.Id);
        if (user is not Infrastructure.Identity.ApplicationUser appUser)
        {
            return Result.Failure(new[] { "User not found." });
        }

        // Find invitation
        var invitation = await _context.Invitations
            .FirstOrDefaultAsync(i => i.Token == request.Token, cancellationToken);

        if (invitation == null)
        {
            return Result.Failure(new[] { "Invitation not found." });
        }

        // Check if invitation is for this user's email
        if (invitation.Email.ToLower() != appUser.Email?.ToLower())
        {
            return Result.Failure(new[] { "This invitation is not for your email address." });
        }

        // Check if invitation is still valid
        if (invitation.Status != InvitationStatus.Pending)
        {
            var statusMessage = invitation.Status switch
            {
                InvitationStatus.Accepted => "This invitation has already been accepted.",
                InvitationStatus.Declined => "This invitation has already been declined.",
                InvitationStatus.Revoked => "This invitation has been revoked.",
                InvitationStatus.Expired => "This invitation has expired.",
                _ => "This invitation is no longer valid."
            };
            return Result.Failure(new[] { statusMessage });
        }

        // Update invitation status
        invitation.Status = InvitationStatus.Declined;

        await _context.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
