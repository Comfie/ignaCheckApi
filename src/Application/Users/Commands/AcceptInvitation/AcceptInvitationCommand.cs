using IgnaCheck.Application.Common.Interfaces;
using IgnaCheck.Domain.Entities;
using IgnaCheck.Domain.Enums;

namespace IgnaCheck.Application.Users.Commands.AcceptInvitation;

/// <summary>
/// Command to accept a workspace invitation.
/// </summary>
public record AcceptInvitationCommand : IRequest<Result<AcceptInvitationResponse>>
{
    /// <summary>
    /// Invitation token.
    /// </summary>
    public string Token { get; init; } = string.Empty;
}

/// <summary>
/// Response for successful invitation acceptance.
/// </summary>
public record AcceptInvitationResponse
{
    /// <summary>
    /// Workspace ID.
    /// </summary>
    public Guid WorkspaceId { get; init; }

    /// <summary>
    /// Workspace name.
    /// </summary>
    public string WorkspaceName { get; init; } = string.Empty;

    /// <summary>
    /// User's role in the workspace.
    /// </summary>
    public string Role { get; init; } = string.Empty;

    /// <summary>
    /// JWT access token with updated organization context.
    /// </summary>
    public string AccessToken { get; init; } = string.Empty;
}

/// <summary>
/// Handler for the AcceptInvitationCommand.
/// </summary>
public class AcceptInvitationCommandHandler : IRequestHandler<AcceptInvitationCommand, Result<AcceptInvitationResponse>>
{
    private readonly IApplicationDbContext _context;
    private readonly IUser _currentUser;
    private readonly IJwtTokenGenerator _jwtTokenGenerator;
    private readonly IIdentityService _identityService;

    public AcceptInvitationCommandHandler(
        IApplicationDbContext context,
        IUser currentUser,
        IJwtTokenGenerator jwtTokenGenerator,
        IIdentityService identityService)
    {
        _context = context;
        _currentUser = currentUser;
        _jwtTokenGenerator = jwtTokenGenerator;
        _identityService = identityService;
    }

    public async Task<Result<AcceptInvitationResponse>> Handle(AcceptInvitationCommand request, CancellationToken cancellationToken)
    {
        // Ensure user is authenticated
        if (string.IsNullOrEmpty(_currentUser.Id))
        {
            return Result<AcceptInvitationResponse>.Failure(new[] { "User must be authenticated to accept an invitation." });
        }

        // Get user details
        var user = await _identityService.GetUserByIdAsync(_currentUser.Id);
        if (user == null)
        {
            return Result<AcceptInvitationResponse>.Failure(new[] { "User not found." });
        }

        // Find invitation
        var invitation = await _context.Invitations
            .Include(i => i.Organization)
            .FirstOrDefaultAsync(i => i.Token == request.Token, cancellationToken);

        if (invitation == null)
        {
            return Result<AcceptInvitationResponse>.Failure(new[] { "Invitation not found." });
        }

        // Check if invitation is for this user's email
        if (invitation.Email.ToLower() != user.Email?.ToLower())
        {
            return Result<AcceptInvitationResponse>.Failure(new[] { "This invitation is not for your email address." });
        }

        // Check if invitation is still valid
        if (invitation.Status != InvitationStatus.Pending)
        {
            var statusMessage = invitation.Status switch
            {
                InvitationStatus.Accepted => "This invitation has already been accepted.",
                InvitationStatus.Declined => "This invitation has been declined.",
                InvitationStatus.Revoked => "This invitation has been revoked.",
                InvitationStatus.Expired => "This invitation has expired.",
                _ => "This invitation is no longer valid."
            };
            return Result<AcceptInvitationResponse>.Failure(new[] { statusMessage });
        }

        // Check if invitation has expired
        if (invitation.ExpiresDate < DateTime.UtcNow)
        {
            invitation.Status = InvitationStatus.Expired;
            await _context.SaveChangesAsync(cancellationToken);
            return Result<AcceptInvitationResponse>.Failure(new[] { "This invitation has expired." });
        }

        // Check if user is already a member
        var existingMembership = await _context.OrganizationMembers
            .FirstOrDefaultAsync(m => m.OrganizationId == invitation.OrganizationId && m.UserId == _currentUser.Id, cancellationToken);

        if (existingMembership != null)
        {
            if (existingMembership.IsActive)
            {
                return Result<AcceptInvitationResponse>.Failure(new[] { "You are already a member of this workspace." });
            }
            else
            {
                // Reactivate membership
                existingMembership.IsActive = true;
                existingMembership.Role = invitation.Role;
                existingMembership.JoinedDate = DateTime.UtcNow;
                existingMembership.InvitationId = invitation.Id;
            }
        }
        else
        {
            // Create new membership
            var member = new OrganizationMember
            {
                Id = Guid.NewGuid(),
                OrganizationId = invitation.OrganizationId,
                UserId = _currentUser.Id,
                Role = invitation.Role,
                JoinedDate = DateTime.UtcNow,
                IsActive = true,
                InvitationId = invitation.Id
            };

            _context.OrganizationMembers.Add(member);
        }

        // Update invitation status
        invitation.Status = InvitationStatus.Accepted;
        invitation.AcceptedDate = DateTime.UtcNow;

        await _context.SaveChangesAsync(cancellationToken);

        // Get user roles for JWT
        var roles = new List<string>(); // TODO: Load actual user roles

        // Generate new JWT token with organization context
        var accessToken = _jwtTokenGenerator.GenerateAccessToken(
            userId: _currentUser.Id,
            email: user.Email!,
            firstName: user.FirstName,
            lastName: user.LastName,
            roles: roles,
            organizationId: invitation.OrganizationId,
            organizationRole: invitation.Role,
            expiresInMinutes: 60
        );

        var response = new AcceptInvitationResponse
        {
            WorkspaceId = invitation.OrganizationId,
            WorkspaceName = invitation.Organization.Name,
            Role = invitation.Role,
            AccessToken = accessToken
        };

        return Result<AcceptInvitationResponse>.Success(response);
    }
}
