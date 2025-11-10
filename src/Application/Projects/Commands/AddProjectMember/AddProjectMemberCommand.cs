using IgnaCheck.Application.Common.Interfaces;
using IgnaCheck.Domain.Entities;
using IgnaCheck.Domain.Enums;

namespace IgnaCheck.Application.Projects.Commands.AddProjectMember;

/// <summary>
/// Command to add a member to a project with a specific role.
/// </summary>
public record AddProjectMemberCommand : IRequest<Result>
{
    /// <summary>
    /// Project ID.
    /// </summary>
    public Guid ProjectId { get; init; }

    /// <summary>
    /// User ID to add (must be a workspace member).
    /// </summary>
    public string UserId { get; init; } = string.Empty;

    /// <summary>
    /// Role to assign to the member.
    /// </summary>
    public ProjectRole Role { get; init; } = ProjectRole.Viewer;
}

/// <summary>
/// Handler for AddProjectMemberCommand.
/// </summary>
public class AddProjectMemberCommandHandler : IRequestHandler<AddProjectMemberCommand, Result>
{
    private readonly IApplicationDbContext _context;
    private readonly IUser _currentUser;
    private readonly ITenantService _tenantService;
    private readonly IIdentityService _identityService;

    public AddProjectMemberCommandHandler(
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

    public async Task<Result> Handle(AddProjectMemberCommand request, CancellationToken cancellationToken)
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

        // Check if current user is project owner (only owners can add members)
        var currentUserMember = project.ProjectMembers.FirstOrDefault(pm => pm.UserId == _currentUser.Id && pm.IsActive);
        if (currentUserMember == null || currentUserMember.Role != ProjectRole.Owner)
        {
            return Result.Failure(new[] { "Only project owners can add members." });
        }

        // Check if user to add is a workspace member
        var organizationMember = await _context.OrganizationMembers
            .FirstOrDefaultAsync(om => om.OrganizationId == organizationId.Value && om.UserId == request.UserId && om.IsActive, cancellationToken);

        if (organizationMember == null)
        {
            return Result.Failure(new[] { "User is not a member of this workspace." });
        }

        // Check if user is already a member
        var existingMember = project.ProjectMembers.FirstOrDefault(pm => pm.UserId == request.UserId);
        if (existingMember != null)
        {
            if (existingMember.IsActive)
            {
                return Result.Failure(new[] { "User is already a member of this project." });
            }
            else
            {
                // Reactivate previously removed member
                existingMember.IsActive = true;
                existingMember.Role = request.Role;
                existingMember.JoinedDate = DateTime.UtcNow;
                existingMember.AddedBy = _currentUser.Id;
                existingMember.RemovedDate = null;
                existingMember.RemovedBy = null;
            }
        }
        else
        {
            // Get user details
            var userToAdd = await _identityService.GetUserByIdAsync(request.UserId);
            if (userToAdd == null)
            {
                return Result.Failure(new[] { "User not found." });
            }

            // Create new project member
            var newMember = new ProjectMember
            {
                Id = Guid.NewGuid(),
                ProjectId = project.Id,
                OrganizationId = organizationId.Value,
                UserId = request.UserId,
                UserName = $"{userToAdd.FirstName} {userToAdd.LastName}".Trim(),
                UserEmail = userToAdd.Email!,
                Role = request.Role,
                JoinedDate = DateTime.UtcNow,
                AddedBy = _currentUser.Id,
                IsActive = true
            };

            _context.ProjectMembers.Add(newMember);
        }

        // Get current user details for activity log
        var currentUser = await _identityService.GetUserByIdAsync(_currentUser.Id);
        if (currentUser == null)
        {
            return Result.Failure(new[] { "Current user not found." });
        }

        var currentUserName = $"{currentUser.FirstName} {currentUser.LastName}".Trim();

        // Get added user details for activity log
        var addedUser = await _identityService.GetUserByIdAsync(request.UserId);
        var addedUserName = addedUser != null
            ? $"{addedUser.FirstName} {addedUser.LastName}".Trim()
            : "Unknown User";

        // Log activity
        var activityLog = new ActivityLog
        {
            Id = Guid.NewGuid(),
            OrganizationId = organizationId.Value,
            ProjectId = project.Id,
            UserId = _currentUser.Id,
            UserName = currentUserName,
            UserEmail = currentUser.Email!,
            ActivityType = ActivityType.ProjectMemberAdded,
            EntityType = "ProjectMember",
            EntityId = request.ProjectId,
            EntityName = project.Name,
            Description = $"Added {addedUserName} to project '{project.Name}' with role '{request.Role}'",
            OccurredAt = DateTime.UtcNow
        };

        _context.ActivityLogs.Add(activityLog);

        await _context.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
