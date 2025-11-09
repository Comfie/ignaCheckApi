using IgnaCheck.Application.Common.Interfaces;
using IgnaCheck.Domain.Entities;

namespace IgnaCheck.Application.Projects.Commands.RemoveProjectMember;

/// <summary>
/// Command to remove a member from a project.
/// </summary>
public record RemoveProjectMemberCommand : IRequest<Result>
{
    /// <summary>
    /// Project ID.
    /// </summary>
    public Guid ProjectId { get; init; }

    /// <summary>
    /// User ID to remove.
    /// </summary>
    public string UserId { get; init; } = string.Empty;
}

/// <summary>
/// Handler for RemoveProjectMemberCommand.
/// </summary>
public class RemoveProjectMemberCommandHandler : IRequestHandler<RemoveProjectMemberCommand, Result>
{
    private readonly IApplicationDbContext _context;
    private readonly IUser _currentUser;
    private readonly ITenantService _tenantService;
    private readonly IIdentityService _identityService;

    public RemoveProjectMemberCommandHandler(
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

    public async Task<Result> Handle(RemoveProjectMemberCommand request, CancellationToken cancellationToken)
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

        // Check if current user is project owner (only owners can remove members)
        var currentUserMember = project.ProjectMembers.FirstOrDefault(pm => pm.UserId == _currentUser.Id && pm.IsActive);
        if (currentUserMember == null || currentUserMember.Role != ProjectRole.Owner)
        {
            return Result.Failure(new[] { "Only project owners can remove members." });
        }

        // Find member to remove
        var memberToRemove = project.ProjectMembers.FirstOrDefault(pm => pm.UserId == request.UserId && pm.IsActive);
        if (memberToRemove == null)
        {
            return Result.Failure(new[] { "User is not a member of this project." });
        }

        // Prevent removing the last owner
        var activeOwners = project.ProjectMembers.Count(pm => pm.IsActive && pm.Role == ProjectRole.Owner);
        if (memberToRemove.Role == ProjectRole.Owner && activeOwners <= 1)
        {
            return Result.Failure(new[] { "Cannot remove the last project owner. Transfer ownership first." });
        }

        // Mark member as inactive
        memberToRemove.IsActive = false;
        memberToRemove.RemovedDate = DateTime.UtcNow;
        memberToRemove.RemovedBy = _currentUser.Id;

        // Get current user details for activity log
        var currentAppUser = await _identityService.GetUserByIdAsync(_currentUser.Id);
        if (currentAppUser is not IgnaCheck.Infrastructure.Identity.ApplicationUser currentUser)
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
            ActivityType = ActivityType.ProjectMemberRemoved,
            EntityType = "ProjectMember",
            EntityId = request.ProjectId,
            EntityName = project.Name,
            Description = $"Removed {memberToRemove.UserName} from project '{project.Name}'",
            OccurredAt = DateTime.UtcNow
        };

        _context.ActivityLogs.Add(activityLog);

        await _context.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
