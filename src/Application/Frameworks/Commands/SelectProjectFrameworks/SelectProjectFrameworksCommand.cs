using IgnaCheck.Application.Common.Interfaces;
using IgnaCheck.Domain.Entities;
using IgnaCheck.Domain.Enums;

namespace IgnaCheck.Application.Frameworks.Commands.SelectProjectFrameworks;

/// <summary>
/// Command to assign/select one or more compliance frameworks to a project.
/// </summary>
public record SelectProjectFrameworksCommand : IRequest<Result>
{
    /// <summary>
    /// Project ID to assign frameworks to.
    /// </summary>
    public Guid ProjectId { get; init; }

    /// <summary>
    /// Framework IDs to assign to the project.
    /// </summary>
    public List<Guid> FrameworkIds { get; init; } = new();

    /// <summary>
    /// Optional target completion date for all frameworks.
    /// </summary>
    public DateTime? TargetCompletionDate { get; init; }

    /// <summary>
    /// Optional notes for the framework assignment.
    /// </summary>
    public string? Notes { get; init; }
}

/// <summary>
/// Handler for SelectProjectFrameworksCommand.
/// </summary>
public class SelectProjectFrameworksCommandHandler : IRequestHandler<SelectProjectFrameworksCommand, Result>
{
    private readonly IApplicationDbContext _context;
    private readonly IUser _currentUser;
    private readonly ITenantService _tenantService;
    private readonly IIdentityService _identityService;

    public SelectProjectFrameworksCommandHandler(
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

    public async Task<Result> Handle(SelectProjectFrameworksCommand request, CancellationToken cancellationToken)
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

        // Validate framework IDs
        if (request.FrameworkIds == null || !request.FrameworkIds.Any())
        {
            return Result.Failure(new[] { "At least one framework must be selected." });
        }

        // Get project with members and existing frameworks
        var project = await _context.Projects
            .Include(p => p.ProjectMembers)
            .Include(p => p.ProjectFrameworks)
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

        // Validate that all frameworks exist and are accessible
        var frameworks = await _context.ComplianceFrameworks
            .Where(f => request.FrameworkIds.Contains(f.Id) &&
                       (f.IsSystemFramework || f.OrganizationId == organizationId.Value) &&
                       f.IsActive)
            .ToListAsync(cancellationToken);

        if (frameworks.Count != request.FrameworkIds.Count)
        {
            var missingIds = request.FrameworkIds.Except(frameworks.Select(f => f.Id)).ToList();
            return Result.Failure(new[] { $"Some frameworks were not found or are not accessible: {string.Join(", ", missingIds)}" });
        }

        // Get user details for activity log
        var user = await _identityService.GetUserByIdAsync(_currentUser.Id);
        if (user == null)
        {
            return Result.Failure(new[] { "User not found." });
        }

        var userName = $"{user.FirstName} {user.LastName}".Trim();

        // Add frameworks (skip if already assigned)
        var addedFrameworks = new List<string>();

        foreach (var framework in frameworks)
        {
            // Check if framework is already assigned
            var existingAssignment = project.ProjectFrameworks
                .FirstOrDefault(pf => pf.FrameworkId == framework.Id && pf.IsActive);

            if (existingAssignment != null)
            {
                // Framework already assigned, skip
                continue;
            }

            // Get control count for this framework
            var controlCount = await _context.ComplianceControls
                .CountAsync(c => c.FrameworkId == framework.Id, cancellationToken);

            // Create new project framework assignment
            var projectFramework = new ProjectFramework
            {
                Id = Guid.NewGuid(),
                ProjectId = project.Id,
                FrameworkId = framework.Id,
                AssignedDate = DateTime.UtcNow,
                TargetCompletionDate = request.TargetCompletionDate,
                Status = ComplianceStatus.NotAssessed,
                CompliancePercentage = 0,
                TotalControlsCount = controlCount,
                NotAssessedControlsCount = controlCount,
                Notes = request.Notes,
                IsActive = true
            };

            _context.ProjectFrameworks.Add(projectFramework);
            addedFrameworks.Add(framework.Name);

            // Log activity
            var activityLog = new ActivityLog
            {
                Id = Guid.NewGuid(),
                OrganizationId = organizationId.Value,
                ProjectId = project.Id,
                UserId = _currentUser.Id,
                UserName = userName,
                UserEmail = user.Email!,
                ActivityType = ActivityType.FrameworkAdded,
                EntityType = "ProjectFramework",
                EntityId = projectFramework.Id,
                EntityName = framework.Name,
                Description = $"Added framework '{framework.Name}' to project '{project.Name}'",
                OccurredAt = DateTime.UtcNow
            };

            _context.ActivityLogs.Add(activityLog);
        }

        if (addedFrameworks.Any())
        {
            await _context.SaveChangesAsync(cancellationToken);
        }

        return addedFrameworks.Any()
            ? Result.Success()
            : Result.Failure(new[] { "All selected frameworks are already assigned to this project." });
    }
}
