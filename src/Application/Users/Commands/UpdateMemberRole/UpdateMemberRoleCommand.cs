using IgnaCheck.Application.Common.Interfaces;

namespace IgnaCheck.Application.Users.Commands.UpdateMemberRole;

/// <summary>
/// Command to update a workspace member's role.
/// </summary>
public record UpdateMemberRoleCommand : IRequest<Result>
{
    /// <summary>
    /// User ID of the member whose role to update.
    /// </summary>
    public string UserId { get; init; } = string.Empty;

    /// <summary>
    /// New role to assign.
    /// </summary>
    public string NewRole { get; init; } = string.Empty;

    /// <summary>
    /// Workspace ID (optional - uses current workspace from JWT if not provided).
    /// </summary>
    public Guid? WorkspaceId { get; init; }
}

/// <summary>
/// Handler for the UpdateMemberRoleCommand.
/// </summary>
public class UpdateMemberRoleCommandHandler : IRequestHandler<UpdateMemberRoleCommand, Result>
{
    private readonly IApplicationDbContext _context;
    private readonly IUser _currentUser;

    public UpdateMemberRoleCommandHandler(
        IApplicationDbContext context,
        IUser currentUser)
    {
        _context = context;
        _currentUser = currentUser;
    }

    public async Task<Result> Handle(UpdateMemberRoleCommand request, CancellationToken cancellationToken)
    {
        // Ensure user is authenticated
        if (string.IsNullOrEmpty(_currentUser.Id))
        {
            return Result.Failure(new[] { "User must be authenticated." });
        }

        // Prevent user from changing their own role
        if (request.UserId == _currentUser.Id)
        {
            return Result.Failure(new[] { "You cannot change your own role." });
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

        // Only Owners and Admins can change roles
        if (currentUserMembership.Role != Domain.Constants.WorkspaceRoles.Owner && currentUserMembership.Role != Domain.Constants.WorkspaceRoles.Admin)
        {
            return Result.Failure(new[] { "Only workspace Owners and Admins can change member roles." });
        }

        // Get target member's membership
        var targetMembership = await _context.OrganizationMembers
            .FirstOrDefaultAsync(m => m.OrganizationId == workspaceId.Value && m.UserId == request.UserId && m.IsActive, cancellationToken);

        if (targetMembership == null)
        {
            return Result.Failure(new[] { "Member not found in this workspace." });
        }

        // Validate role hierarchy
        var currentUserRoleLevel = Domain.Constants.WorkspaceRoles.GetHierarchyLevel(currentUserMembership.Role);
        var targetCurrentRoleLevel = Domain.Constants.WorkspaceRoles.GetHierarchyLevel(targetMembership.Role);
        var newRoleLevel = Domain.Constants.WorkspaceRoles.GetHierarchyLevel(request.NewRole);

        // Cannot change role of someone with equal or higher role
        if (targetCurrentRoleLevel <= currentUserRoleLevel)
        {
            return Result.Failure(new[] { "You cannot change the role of a member with equal or higher privileges." });
        }

        // Cannot assign a role equal to or higher than your own (except Owners can assign Owner)
        if (currentUserMembership.Role != Domain.Constants.WorkspaceRoles.Owner && newRoleLevel <= currentUserRoleLevel)
        {
            return Result.Failure(new[] { "You cannot assign a role equal to or higher than your own." });
        }

        // Special rule: Only Owners can assign/change Owner role
        if (request.NewRole == Domain.Constants.WorkspaceRoles.Owner && currentUserMembership.Role != Domain.Constants.WorkspaceRoles.Owner)
        {
            return Result.Failure(new[] { "Only workspace Owners can assign the Owner role." });
        }

        // Ensure at least one Owner remains if changing from Owner
        if (targetMembership.Role == Domain.Constants.WorkspaceRoles.Owner)
        {
            var ownerCount = await _context.OrganizationMembers
                .CountAsync(m => m.OrganizationId == workspaceId.Value &&
                                m.Role == Domain.Constants.WorkspaceRoles.Owner &&
                                m.IsActive,
                           cancellationToken);

            if (ownerCount <= 1)
            {
                return Result.Failure(new[] { "Cannot change role. The workspace must have at least one Owner." });
            }
        }

        // Update role
        targetMembership.Role = request.NewRole;

        await _context.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
