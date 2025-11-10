using IgnaCheck.Application.Common.Interfaces;
using IgnaCheck.Domain.Entities;
using IgnaCheck.Domain.Enums;

namespace IgnaCheck.Application.Projects.Commands.UpdateProject;

/// <summary>
/// Command to update an existing project.
/// </summary>
public record UpdateProjectCommand : IRequest<Result<UpdateProjectResponse>>
{
    /// <summary>
    /// Project ID to update.
    /// </summary>
    public Guid ProjectId { get; init; }

    /// <summary>
    /// Updated project name.
    /// </summary>
    public string Name { get; init; } = string.Empty;

    /// <summary>
    /// Updated project description.
    /// </summary>
    public string? Description { get; init; }

    /// <summary>
    /// Updated project status.
    /// </summary>
    public ProjectStatus? Status { get; init; }

    /// <summary>
    /// Updated target date.
    /// </summary>
    public DateTime? TargetDate { get; init; }
}

/// <summary>
/// Response for successful project update.
/// </summary>
public record UpdateProjectResponse
{
    public Guid Id { get; init; }
    public string Name { get; init; } = string.Empty;
    public string? Description { get; init; }
    public ProjectStatus Status { get; init; }
    public DateTime? TargetDate { get; init; }
    public DateTime LastModified { get; init; }
}

/// <summary>
/// Handler for UpdateProjectCommand.
/// </summary>
public class UpdateProjectCommandHandler : IRequestHandler<UpdateProjectCommand, Result<UpdateProjectResponse>>
{
    private readonly IApplicationDbContext _context;
    private readonly IUser _currentUser;
    private readonly ITenantService _tenantService;
    private readonly IIdentityService _identityService;

    public UpdateProjectCommandHandler(
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

    public async Task<Result<UpdateProjectResponse>> Handle(UpdateProjectCommand request, CancellationToken cancellationToken)
    {
        // Ensure user is authenticated
        if (string.IsNullOrEmpty(_currentUser.Id))
        {
            return Result<UpdateProjectResponse>.Failure(new[] { "User must be authenticated." });
        }

        // Get current organization
        var organizationId = _tenantService.GetCurrentTenantId();
        if (organizationId == null)
        {
            return Result<UpdateProjectResponse>.Failure(new[] { "No workspace selected." });
        }

        // Get project with member info
        var project = await _context.Projects
            .Include(p => p.ProjectMembers)
            .FirstOrDefaultAsync(p => p.Id == request.ProjectId && p.OrganizationId == organizationId.Value, cancellationToken);

        if (project == null)
        {
            return Result<UpdateProjectResponse>.Failure(new[] { "Project not found." });
        }

        // Check if user has permission to update (must be Owner or Contributor)
        var userMember = project.ProjectMembers.FirstOrDefault(pm => pm.UserId == _currentUser.Id && pm.IsActive);
        if (userMember == null || userMember.Role == ProjectRole.Viewer)
        {
            return Result<UpdateProjectResponse>.Failure(new[] { "You do not have permission to update this project." });
        }

        // Get user details for activity log
        var user = await _identityService.GetUserByIdAsync(_currentUser.Id);
        if (user is not IgnaCheck.Infrastructure.Identity.ApplicationUser appUser)
        {
            return Result<UpdateProjectResponse>.Failure(new[] { "User not found." });
        }

        var userName = $"{appUser.FirstName} {appUser.LastName}".Trim();

        // Track changes for activity log
        var changes = new List<string>();

        if (project.Name != request.Name.Trim())
        {
            changes.Add($"name from '{project.Name}' to '{request.Name.Trim()}'");
            project.Name = request.Name.Trim();
        }

        if (project.Description != request.Description?.Trim())
        {
            changes.Add("description");
            project.Description = request.Description?.Trim();
        }

        if (request.Status.HasValue && project.Status != request.Status.Value)
        {
            changes.Add($"status from '{project.Status}' to '{request.Status.Value}'");
            project.Status = request.Status.Value;
        }

        if (project.TargetDate != request.TargetDate)
        {
            changes.Add("target date");
            project.TargetDate = request.TargetDate;
        }

        // Only save if there are changes
        if (changes.Any())
        {
            // Log activity
            var activityLog = new ActivityLog
            {
                Id = Guid.NewGuid(),
                OrganizationId = organizationId.Value,
                ProjectId = project.Id,
                UserId = _currentUser.Id,
                UserName = userName,
                UserEmail = appUser.Email!,
                ActivityType = ActivityType.ProjectUpdated,
                EntityType = "Project",
                EntityId = project.Id,
                EntityName = project.Name,
                Description = $"Updated project '{project.Name}': {string.Join(", ", changes)}",
                OccurredAt = DateTime.UtcNow
            };

            _context.ActivityLogs.Add(activityLog);

            await _context.SaveChangesAsync(cancellationToken);
        }

        var response = new UpdateProjectResponse
        {
            Id = project.Id,
            Name = project.Name,
            Description = project.Description,
            Status = project.Status,
            TargetDate = project.TargetDate,
            LastModified = project.LastModified
        };

        return Result<UpdateProjectResponse>.Success(response);
    }
}
