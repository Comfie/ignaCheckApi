using IgnaCheck.Application.Common.Interfaces;
using IgnaCheck.Domain.Entities;
using IgnaCheck.Domain.Enums;

namespace IgnaCheck.Application.Frameworks.Commands.RemoveProjectFramework;

/// <summary>
/// Command to remove a compliance framework from a project.
/// </summary>
public record RemoveProjectFrameworkCommand : IRequest<Result>
{
    /// <summary>
    /// Project ID to remove framework from.
    /// </summary>
    public Guid ProjectId { get; init; }

    /// <summary>
    /// Framework ID to remove.
    /// </summary>
    public Guid FrameworkId { get; init; }
}

/// <summary>
/// Handler for RemoveProjectFrameworkCommand.
/// </summary>
public class RemoveProjectFrameworkCommandHandler : IRequestHandler<RemoveProjectFrameworkCommand, Result>
{
    private readonly IApplicationDbContext _context;
    private readonly IUser _currentUser;
    private readonly ITenantService _tenantService;
    private readonly IIdentityService _identityService;

    public RemoveProjectFrameworkCommandHandler(
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

    public async Task<Result> Handle(RemoveProjectFrameworkCommand request, CancellationToken cancellationToken)
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

        // Get project with members and frameworks
        var project = await _context.Projects
            .Include(p => p.ProjectMembers)
            .Include(p => p.ProjectFrameworks).ThenInclude(pf => pf.Framework)
            .FirstOrDefaultAsync(p => p.Id == request.ProjectId && p.OrganizationId == organizationId.Value, cancellationToken);

        if (project == null)
        {
            return Result.Failure(new[] { "Project not found." });
        }

        // Check if user has permission (Owner or Contributor)
        var userMember = project.ProjectMembers.FirstOrDefault(pm => pm.UserId == _currentUser.Id && pm.IsActive);
        if (userMember == null || userMember.Role == ProjectRole.Viewer)
        {
            return Result.Failure(new[] { "You do not have permission to modify project frameworks." });
        }

        // Find the project framework assignment
        var projectFramework = project.ProjectFrameworks
            .FirstOrDefault(pf => pf.FrameworkId == request.FrameworkId && pf.IsActive);

        if (projectFramework == null)
        {
            return Result.Failure(new[] { "Framework is not assigned to this project." });
        }

        // Check if there are any findings associated with this framework
        var findingsCount = await _context.ComplianceFindings
            .CountAsync(f => f.ProjectId == project.Id &&
                            f.Control.FrameworkId == request.FrameworkId,
                       cancellationToken);

        if (findingsCount > 0)
        {
            return Result.Failure(new[] { $"Cannot remove framework. There are {findingsCount} findings associated with this framework. Please resolve or delete them first." });
        }

        // Get user details for activity log
        var user = await _identityService.GetUserByIdAsync(_currentUser.Id);
        if (user is not IgnaCheck.Infrastructure.Identity.ApplicationUser appUser)
        {
            return Result.Failure(new[] { "User not found." });
        }

        var userName = $"{appUser.FirstName} {appUser.LastName}".Trim();

        // Remove the framework assignment
        _context.ProjectFrameworks.Remove(projectFramework);

        // Log activity
        var activityLog = new ActivityLog
        {
            Id = Guid.NewGuid(),
            OrganizationId = organizationId.Value,
            ProjectId = project.Id,
            UserId = _currentUser.Id,
            UserName = userName,
            UserEmail = appUser.Email!,
            ActivityType = ActivityType.FrameworkRemoved,
            EntityType = "ProjectFramework",
            EntityId = projectFramework.Id,
            EntityName = projectFramework.Framework.Name,
            Description = $"Removed framework '{projectFramework.Framework.Name}' from project '{project.Name}'",
            OccurredAt = DateTime.UtcNow
        };

        _context.ActivityLogs.Add(activityLog);

        await _context.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
