using IgnaCheck.Application.Common.Interfaces;
using IgnaCheck.Domain.Entities;
using IgnaCheck.Domain.Enums;

namespace IgnaCheck.Application.Projects.Commands.UpdateProjectMemberRole;

/// <summary>
/// Command to update a project member's role.
/// </summary>
public record UpdateProjectMemberRoleCommand : IRequest<Result>
{
    /// <summary>
    /// Project ID.
    /// </summary>
    public Guid ProjectId { get; init; }

    /// <summary>
    /// User ID whose role to update.
    /// </summary>
    public string UserId { get; init; } = string.Empty;

    /// <summary>
    /// New role to assign.
    /// </summary>
    public ProjectRole NewRole { get; init; }
}

/// <summary>
/// Handler for UpdateProjectMemberRoleCommand.
/// </summary>
public class UpdateProjectMemberRoleCommandHandler : IRequestHandler<UpdateProjectMemberRoleCommand, Result>
{
    private readonly IApplicationDbContext _context;
    private readonly IUser _currentUser;
    private readonly ITenantService _tenantService;
    private readonly IIdentityService _identityService;

    public UpdateProjectMemberRoleCommandHandler(
        IApplicationDbContext context,
        IUser currentUser,
        ITenantService tenantService,
        IIdentityService identityService)
    {
        _context = context;
        _currentUser = currentUser;
        _tenantService = tenantService;
        _identityService = identityService;
    }

    public async Task<Result> Handle(UpdateProjectMemberRoleCommand request, CancellationToken cancellationToken)
    {
        // Ensure user is authenticated
        if (string.IsNullOrEmpty(_currentUser.Id))
        {
            return Result.Failure(new[] { "User must be authenticated." });
        }

        // Get current organization
        var organizationId = _tenantService.GetCurrentTenantId();
        if (organizationId == null)
        {
            return Result.Failure(new[] { "No workspace selected." });
        }

        // Get project with members
        var project = await _context.Projects
            .Include(p => p.ProjectMembers)
            .FirstOrDefaultAsync(p => p.Id == request.ProjectId && p.OrganizationId == organizationId.Value, cancellationToken);

        if (project == null)
        {
            return Result.Failure(new[] { "Project not found." });
        }

        // Check if current user is project owner (only owners can change roles)
        var currentUserMember = project.ProjectMembers.FirstOrDefault(pm => pm.UserId == _currentUser.Id && pm.IsActive);
        if (currentUserMember == null || currentUserMember.Role != ProjectRole.Owner)
        {
            return Result.Failure(new[] { "Only project owners can change member roles." });
        }

        // Find member to update
        var memberToUpdate = project.ProjectMembers.FirstOrDefault(pm => pm.UserId == request.UserId && pm.IsActive);
        if (memberToUpdate == null)
        {
            return Result.Failure(new[] { "User is not a member of this project." });
        }

        // Check if role is actually changing
        if (memberToUpdate.Role == request.NewRole)
        {
            return Result.Failure(new[] { $"User already has the role '{request.NewRole}'." });
        }

        // Prevent demoting the last owner
        if (memberToUpdate.Role == ProjectRole.Owner && request.NewRole != ProjectRole.Owner)
        {
            var activeOwners = project.ProjectMembers.Count(pm => pm.IsActive && pm.Role == ProjectRole.Owner);
            if (activeOwners <= 1)
            {
                return Result.Failure(new[] { "Cannot demote the last project owner. Assign another owner first." });
            }
        }

        var oldRole = memberToUpdate.Role;
        memberToUpdate.Role = request.NewRole;

        // Get current user details for activity log
        var currentUser = await _identityService.GetUserByIdAsync(_currentUser.Id);
        if (currentUser == null)
        {
            return Result.Failure(new[] { "Current user not found." });
        }

        var currentUserName = $"{currentUser.FirstName} {currentUser.LastName}".Trim();

        // Log activity
        var activityLog = new ActivityLog
        {
            Id = Guid.NewGuid(),
            OrganizationId = organizationId.Value,
            ProjectId = project.Id,
            UserId = _currentUser.Id,
            UserName = currentUserName,
            UserEmail = currentUser.Email!,
            ActivityType = ActivityType.ProjectMemberRoleChanged,
            EntityType = "ProjectMember",
            EntityId = request.ProjectId,
            EntityName = project.Name,
            Description = $"Changed {memberToUpdate.UserName}'s role from '{oldRole}' to '{request.NewRole}' in project '{project.Name}'",
            OccurredAt = DateTime.UtcNow
        };

        _context.ActivityLogs.Add(activityLog);

        await _context.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
