using IgnaCheck.Application.Common.Interfaces;
using IgnaCheck.Domain.Entities;
using IgnaCheck.Domain.Enums;

namespace IgnaCheck.Application.Projects.Commands.ArchiveProject;

/// <summary>
/// Command to archive or restore a project.
/// Archived projects are hidden by default but can be restored.
/// </summary>
public record ArchiveProjectCommand : IRequest<Result>
{
    /// <summary>
    /// Project ID to archive/restore.
    /// </summary>
    public Guid ProjectId { get; init; }

    /// <summary>
    /// True to archive, false to restore.
    /// </summary>
    public bool Archive { get; init; } = true;
}

/// <summary>
/// Handler for ArchiveProjectCommand.
/// </summary>
public class ArchiveProjectCommandHandler : IRequestHandler<ArchiveProjectCommand, Result>
{
    private readonly IApplicationDbContext _context;
    private readonly IUser _currentUser;
    private readonly ITenantService _tenantService;
    private readonly IIdentityService _identityService;

    public ArchiveProjectCommandHandler(
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

    public async Task<Result> Handle(ArchiveProjectCommand request, CancellationToken cancellationToken)
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

        // Check if user is project owner (only owners can archive/restore)
        var userMember = project.ProjectMembers.FirstOrDefault(pm => pm.UserId == _currentUser.Id && pm.IsActive);
        if (userMember == null || userMember.Role != ProjectRole.Owner)
        {
            return Result.Failure(new[] { "Only project owners can archive or restore projects." });
        }

        // Check current state
        if (request.Archive && project.Status == ProjectStatus.Archived)
        {
            return Result.Failure(new[] { "Project is already archived." });
        }

        if (!request.Archive && project.Status != ProjectStatus.Archived)
        {
            return Result.Failure(new[] { "Project is not archived." });
        }

        // Get user details for activity log
        var user = await _identityService.GetUserByIdAsync(_currentUser.Id);
        if (user == null)
        {
            return Result.Failure(new[] { "User not found." });
        }

        var userName = $"{user.FirstName} {user.LastName}".Trim();

        // Archive or restore
        var previousStatus = project.Status;

        if (request.Archive)
        {
            project.Status = ProjectStatus.Archived;

            var activityLog = new ActivityLog
            {
                Id = Guid.NewGuid(),
                OrganizationId = organizationId.Value,
                ProjectId = project.Id,
                UserId = _currentUser.Id,
                UserName = userName,
                UserEmail = user.Email!,
                ActivityType = ActivityType.ProjectArchived,
                EntityType = "Project",
                EntityId = project.Id,
                EntityName = project.Name,
                Description = $"Archived project '{project.Name}'",
                OccurredAt = DateTime.UtcNow
            };

            _context.ActivityLogs.Add(activityLog);
        }
        else
        {
            // Restore to Active status
            project.Status = ProjectStatus.Active;

            var activityLog = new ActivityLog
            {
                Id = Guid.NewGuid(),
                OrganizationId = organizationId.Value,
                ProjectId = project.Id,
                UserId = _currentUser.Id,
                UserName = userName,
                UserEmail = user.Email!,
                ActivityType = ActivityType.ProjectRestored,
                EntityType = "Project",
                EntityId = project.Id,
                EntityName = project.Name,
                Description = $"Restored project '{project.Name}' from archive",
                OccurredAt = DateTime.UtcNow
            };

            _context.ActivityLogs.Add(activityLog);
        }

        await _context.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
