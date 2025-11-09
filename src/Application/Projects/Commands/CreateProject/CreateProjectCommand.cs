using IgnaCheck.Application.Common.Interfaces;
using IgnaCheck.Domain.Entities;
using IgnaCheck.Domain.Enums;

namespace IgnaCheck.Application.Projects.Commands.CreateProject;

/// <summary>
/// Command to create a new project within the current workspace.
/// </summary>
public record CreateProjectCommand : IRequest<Result<CreateProjectResponse>>
{
    /// <summary>
    /// Project name (required, 3-100 characters).
    /// </summary>
    public string Name { get; init; } = string.Empty;

    /// <summary>
    /// Project description (optional, max 500 characters).
    /// </summary>
    public string? Description { get; init; }

    /// <summary>
    /// Target date for project completion (optional).
    /// </summary>
    public DateTime? TargetDate { get; init; }
}

/// <summary>
/// Response for successful project creation.
/// </summary>
public record CreateProjectResponse
{
    public Guid Id { get; init; }
    public string Name { get; init; } = string.Empty;
    public string? Description { get; init; }
    public ProjectStatus Status { get; init; }
    public DateTime? TargetDate { get; init; }
    public DateTime CreatedAt { get; init; }
    public string CreatedBy { get; init; } = string.Empty;
}

/// <summary>
/// Handler for the CreateProjectCommand.
/// </summary>
public class CreateProjectCommandHandler : IRequestHandler<CreateProjectCommand, Result<CreateProjectResponse>>
{
    private readonly IApplicationDbContext _context;
    private readonly IUser _currentUser;
    private readonly ITenantService _tenantService;
    private readonly IIdentityService _identityService;

    public CreateProjectCommandHandler(
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

    public async Task<Result<CreateProjectResponse>> Handle(CreateProjectCommand request, CancellationToken cancellationToken)
    {
        // Ensure user is authenticated
        if (string.IsNullOrEmpty(_currentUser.Id))
        {
            return Result<CreateProjectResponse>.Failure(new[] { "User must be authenticated to create a project." });
        }

        // Get current organization from tenant context
        var organizationId = _tenantService.GetCurrentTenantId();
        if (organizationId == null)
        {
            return Result<CreateProjectResponse>.Failure(new[] { "No workspace selected. Please select a workspace first." });
        }

        // Verify organization exists and is active
        var organization = await _context.Organizations
            .FirstOrDefaultAsync(o => o.Id == organizationId.Value, cancellationToken);

        if (organization == null)
        {
            return Result<CreateProjectResponse>.Failure(new[] { "Workspace not found." });
        }

        if (!organization.IsActive)
        {
            return Result<CreateProjectResponse>.Failure(new[] { "Workspace is not active." });
        }

        // Check project limits based on subscription
        if (organization.MaxProjects.HasValue)
        {
            var currentProjectCount = await _context.Projects
                .CountAsync(p => p.OrganizationId == organizationId.Value, cancellationToken);

            if (currentProjectCount >= organization.MaxProjects.Value)
            {
                return Result<CreateProjectResponse>.Failure(new[]
                {
                    $"Project limit reached. Your {organization.SubscriptionTier} plan allows {organization.MaxProjects.Value} projects."
                });
            }
        }

        // Get user details for project member
        var user = await _identityService.GetUserByIdAsync(_currentUser.Id);
        if (user is not IgnaCheck.Infrastructure.Identity.ApplicationUser appUser)
        {
            return Result<CreateProjectResponse>.Failure(new[] { "User not found." });
        }

        // Create project
        var project = new Project
        {
            Id = Guid.NewGuid(),
            Name = request.Name.Trim(),
            Description = request.Description?.Trim(),
            Status = ProjectStatus.Draft,
            TargetDate = request.TargetDate,
            OrganizationId = organizationId.Value
        };

        _context.Projects.Add(project);

        // Add creator as project owner
        var projectMember = new ProjectMember
        {
            Id = Guid.NewGuid(),
            ProjectId = project.Id,
            OrganizationId = organizationId.Value,
            UserId = _currentUser.Id,
            UserName = $"{appUser.FirstName} {appUser.LastName}".Trim(),
            UserEmail = appUser.Email!,
            Role = ProjectRole.Owner,
            JoinedDate = DateTime.UtcNow,
            AddedBy = _currentUser.Id,
            IsActive = true
        };

        _context.ProjectMembers.Add(projectMember);

        // Log activity
        var activityLog = new ActivityLog
        {
            Id = Guid.NewGuid(),
            OrganizationId = organizationId.Value,
            ProjectId = project.Id,
            UserId = _currentUser.Id,
            UserName = projectMember.UserName,
            UserEmail = projectMember.UserEmail,
            ActivityType = ActivityType.ProjectCreated,
            EntityType = "Project",
            EntityId = project.Id,
            EntityName = project.Name,
            Description = $"Created project '{project.Name}'",
            OccurredAt = DateTime.UtcNow
        };

        _context.ActivityLogs.Add(activityLog);

        await _context.SaveChangesAsync(cancellationToken);

        var response = new CreateProjectResponse
        {
            Id = project.Id,
            Name = project.Name,
            Description = project.Description,
            Status = project.Status,
            TargetDate = project.TargetDate,
            CreatedAt = project.Created,
            CreatedBy = projectMember.UserName
        };

        return Result<CreateProjectResponse>.Success(response);
    }
}
