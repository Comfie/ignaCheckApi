using IgnaCheck.Application.Common.Interfaces;

namespace IgnaCheck.Application.Users.Commands.RemoveMember;

/// <summary>
/// Command to remove a member from a workspace.
/// </summary>
public record RemoveMemberCommand : IRequest<Result>
{
    /// <summary>
    /// User ID of the member to remove.
    /// </summary>
    public string UserId { get; init; } = string.Empty;

    /// <summary>
    /// Workspace ID (optional - uses current workspace from JWT if not provided).
    /// </summary>
    public Guid? WorkspaceId { get; init; }
}

/// <summary>
/// Handler for the RemoveMemberCommand.
/// </summary>
public class RemoveMemberCommandHandler : IRequestHandler<RemoveMemberCommand, Result>
{
    private readonly IApplicationDbContext _context;
    private readonly IUser _currentUser;

    public RemoveMemberCommandHandler(
        IApplicationDbContext context,
        IUser currentUser)
    {
        _context = context;
        _currentUser = currentUser;
    }

    public async Task<Result> Handle(RemoveMemberCommand request, CancellationToken cancellationToken)
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

        // Get current user's membership
        var currentUserMembership = await _context.OrganizationMembers
            .FirstOrDefaultAsync(m => m.OrganizationId == workspaceId.Value && m.UserId == _currentUser.Id && m.IsActive, cancellationToken);

        if (currentUserMembership == null)
        {
            return Result.Failure(new[] { "You are not a member of this workspace." });
        }

        // Get target member's membership
        var targetMembership = await _context.OrganizationMembers
            .FirstOrDefaultAsync(m => m.OrganizationId == workspaceId.Value && m.UserId == request.UserId && m.IsActive, cancellationToken);

        if (targetMembership == null)
        {
            return Result.Failure(new[] { "Member not found in this workspace." });
        }

        // Check permissions
        // Users can remove themselves (leave workspace)
        // Owners and Admins can remove others (with hierarchy restrictions)
        if (request.UserId != _currentUser.Id)
        {
            // Only Owners and Admins can remove others
            if (currentUserMembership.Role != Domain.Constants.WorkspaceRoles.Owner && currentUserMembership.Role != Domain.Constants.WorkspaceRoles.Admin)
            {
                return Result.Failure(new[] { "Only workspace Owners and Admins can remove members." });
            }

            // Validate role hierarchy - cannot remove someone with equal or higher role
            var currentUserRoleLevel = Domain.Constants.WorkspaceRoles.GetHierarchyLevel(currentUserMembership.Role);
            var targetRoleLevel = Domain.Constants.WorkspaceRoles.GetHierarchyLevel(targetMembership.Role);

            if (targetRoleLevel <= currentUserRoleLevel)
            {
                return Result.Failure(new[] { "You cannot remove a member with equal or higher privileges." });
            }
        }

        // Ensure at least one Owner remains
        if (targetMembership.Role == Domain.Constants.WorkspaceRoles.Owner)
        {
            var ownerCount = await _context.OrganizationMembers
                .CountAsync(m => m.OrganizationId == workspaceId.Value &&
                                m.Role == Domain.Constants.WorkspaceRoles.Owner &&
                                m.IsActive,
                           cancellationToken);

            if (ownerCount <= 1)
            {
                return Result.Failure(new[] { "Cannot remove the last Owner. Transfer ownership first or delete the workspace." });
            }
        }

        // Mark membership as inactive (soft delete)
        targetMembership.IsActive = false;

        await _context.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
