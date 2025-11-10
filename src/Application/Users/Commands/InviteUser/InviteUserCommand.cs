using IgnaCheck.Application.Common.Interfaces;
using IgnaCheck.Domain.Entities;
using IgnaCheck.Domain.Enums;

namespace IgnaCheck.Application.Users.Commands.InviteUser;

/// <summary>
/// Command to invite a user to a workspace.
/// </summary>
public record InviteUserCommand : IRequest<Result<InviteUserResponse>>
{
    /// <summary>
    /// Email address of the user to invite.
    /// </summary>
    public string Email { get; init; } = string.Empty;

    /// <summary>
    /// Role to assign to the invited user.
    /// </summary>
    public string Role { get; init; } = string.Empty;

    /// <summary>
    /// Optional personal message to include in the invitation email.
    /// </summary>
    public string? Message { get; init; }

    /// <summary>
    /// Workspace ID (optional - uses current workspace from JWT if not provided).
    /// </summary>
    public Guid? WorkspaceId { get; init; }
}

/// <summary>
/// Response for successful user invitation.
/// </summary>
public record InviteUserResponse
{
    /// <summary>
    /// Invitation ID.
    /// </summary>
    public Guid Id { get; init; }

    /// <summary>
    /// Email address of the invited user.
    /// </summary>
    public string Email { get; init; } = string.Empty;

    /// <summary>
    /// Role assigned to the invited user.
    /// </summary>
    public string Role { get; init; } = string.Empty;

    /// <summary>
    /// Invitation expiration date.
    /// </summary>
    public DateTime ExpiresDate { get; init; }

    /// <summary>
    /// Workspace name.
    /// </summary>
    public string WorkspaceName { get; init; } = string.Empty;
}

/// <summary>
/// Handler for the InviteUserCommand.
/// </summary>
public class InviteUserCommandHandler : IRequestHandler<InviteUserCommand, Result<InviteUserResponse>>
{
    private readonly IApplicationDbContext _context;
    private readonly IUser _currentUser;
    private readonly IEmailService _emailService;
    private readonly IIdentityService _identityService;

    public InviteUserCommandHandler(
        IApplicationDbContext context,
        IUser currentUser,
        IEmailService emailService,
        IIdentityService identityService)
    {
        _context = context;
        _currentUser = currentUser;
        _emailService = emailService;
        _identityService = identityService;
    }

    public async Task<Result<InviteUserResponse>> Handle(InviteUserCommand request, CancellationToken cancellationToken)
    {
        // Ensure user is authenticated
        if (string.IsNullOrEmpty(_currentUser.Id))
        {
            return Result<InviteUserResponse>.Failure(new[] { "User must be authenticated." });
        }

        // Determine workspace ID
        var workspaceId = request.WorkspaceId ?? _currentUser.OrganizationId;
        if (!workspaceId.HasValue)
        {
            return Result<InviteUserResponse>.Failure(new[] { "Workspace ID is required." });
        }

        // Get organization
        var organization = await _context.Organizations
            .FirstOrDefaultAsync(o => o.Id == workspaceId.Value, cancellationToken);

        if (organization == null)
        {
            return Result<InviteUserResponse>.Failure(new[] { "Workspace not found." });
        }

        // Check if current user has permission to invite (must be Owner or Admin)
        var currentUserMembership = await _context.OrganizationMembers
            .FirstOrDefaultAsync(m => m.OrganizationId == workspaceId.Value && m.UserId == _currentUser.Id && m.IsActive, cancellationToken);

        if (currentUserMembership == null)
        {
            return Result<InviteUserResponse>.Failure(new[] { "You are not a member of this workspace." });
        }

        if (currentUserMembership.Role != Domain.Constants.WorkspaceRoles.Owner && currentUserMembership.Role != Domain.Constants.WorkspaceRoles.Admin)
        {
            return Result<InviteUserResponse>.Failure(new[] { "Only workspace Owners and Admins can invite users." });
        }

        // Validate role hierarchy (cannot invite someone with higher or equal role)
        var currentUserRoleLevel = Domain.Constants.WorkspaceRoles.GetHierarchyLevel(currentUserMembership.Role);
        var inviteRoleLevel = Domain.Constants.WorkspaceRoles.GetHierarchyLevel(request.Role);

        if (inviteRoleLevel <= currentUserRoleLevel)
        {
            return Result<InviteUserResponse>.Failure(new[] { "You cannot invite users with a role equal to or higher than your own." });
        }

        // Check if user with this email already exists and is already a member
        var existingUser = await _identityService.GetUserByEmailAsync(request.Email);
        OrganizationMember? existingMembership = null;

        if (existingUser != null)
        {
            existingMembership = await _context.OrganizationMembers
                .FirstOrDefaultAsync(m => m.OrganizationId == workspaceId.Value && m.UserId == existingUser.Id, cancellationToken);
        }

        if (existingMembership != null)
        {
            if (existingMembership.IsActive)
            {
                return Result<InviteUserResponse>.Failure(new[] { "This user is already a member of the workspace." });
            }
            else
            {
                return Result<InviteUserResponse>.Failure(new[] { "This user was previously removed from the workspace." });
            }
        }

        // Check for existing pending invitation
        var existingInvitation = await _context.Invitations
            .FirstOrDefaultAsync(i => i.OrganizationId == workspaceId.Value &&
                                     i.Email.ToLower() == request.Email.ToLower() &&
                                     i.Status == InvitationStatus.Pending,
                                     cancellationToken);

        if (existingInvitation != null)
        {
            // If invitation is expired, revoke it
            if (existingInvitation.ExpiresDate < DateTime.UtcNow)
            {
                existingInvitation.Status = InvitationStatus.Expired;
            }
            else
            {
                return Result<InviteUserResponse>.Failure(new[] { "There is already a pending invitation for this email address." });
            }
        }

        // Check workspace member limit
        if (organization.MaxMembers.HasValue)
        {
            var currentMemberCount = await _context.OrganizationMembers
                .CountAsync(m => m.OrganizationId == workspaceId.Value && m.IsActive, cancellationToken);

            if (currentMemberCount >= organization.MaxMembers.Value)
            {
                return Result<InviteUserResponse>.Failure(new[] { $"Workspace has reached its member limit of {organization.MaxMembers.Value}. Please upgrade your subscription." });
            }
        }

        // Get inviter details
        var inviter = await _identityService.GetUserByIdAsync(_currentUser.Id);
        var inviterName = inviter is Infrastructure.Identity.ApplicationUser appUser
            ? appUser.FullName
            : "A team member";

        // Create invitation
        var invitation = new Invitation
        {
            Id = Guid.NewGuid(),
            OrganizationId = workspaceId.Value,
            Email = request.Email.ToLower(),
            Role = request.Role,
            Token = GenerateInvitationToken(),
            ExpiresDate = DateTime.UtcNow.AddDays(7),
            Status = InvitationStatus.Pending,
            InvitedBy = _currentUser.Id,
            Message = request.Message
        };

        _context.Invitations.Add(invitation);
        await _context.SaveChangesAsync(cancellationToken);

        // Send invitation email
        await _emailService.SendWorkspaceInvitationAsync(
            email: request.Email,
            firstName: null, // Don't know first name yet
            organizationName: organization.Name,
            inviterName: inviterName,
            role: request.Role,
            invitationToken: invitation.Token,
            message: request.Message,
            cancellationToken: cancellationToken
        );

        var response = new InviteUserResponse
        {
            Id = invitation.Id,
            Email = invitation.Email,
            Role = invitation.Role,
            ExpiresDate = invitation.ExpiresDate,
            WorkspaceName = organization.Name
        };

        return Result<InviteUserResponse>.Success(response);
    }

    private static string GenerateInvitationToken()
    {
        return Convert.ToBase64String(Guid.NewGuid().ToByteArray())
            .Replace("+", "-")
            .Replace("/", "_")
            .Replace("=", "");
    }
}
