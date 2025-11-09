using IgnaCheck.Application.Common.Interfaces;
using IgnaCheck.Domain.Enums;

namespace IgnaCheck.Application.Projects.Queries.GetProjectsList;

/// <summary>
/// Query to get all projects accessible by the current user.
/// </summary>
public record GetProjectsListQuery : IRequest<Result<List<ProjectDto>>>
{
    /// <summary>
    /// Filter by project status (optional).
    /// </summary>
    public ProjectStatus? Status { get; init; }

    /// <summary>
    /// Search by project name or description (optional).
    /// </summary>
    public string? SearchTerm { get; init; }

    /// <summary>
    /// Filter to show only projects where user is owner/contributor (default: all accessible).
    /// </summary>
    public bool MyProjectsOnly { get; init; } = false;

    /// <summary>
    /// Include archived projects (default: false).
    /// </summary>
    public bool IncludeArchived { get; init; } = false;
}

/// <summary>
/// Project DTO for list display.
/// </summary>
public record ProjectDto
{
    public Guid Id { get; init; }
    public string Name { get; init; } = string.Empty;
    public string? Description { get; init; }
    public ProjectStatus Status { get; init; }
    public DateTime? TargetDate { get; init; }
    public DateTime CreatedAt { get; init; }
    public DateTime? LastModified { get; init; }
    public int DocumentCount { get; init; }
    public int FindingsCount { get; init; }
    public int FrameworkCount { get; init; }
    public string UserRole { get; init; } = string.Empty;
}

/// <summary>
/// Handler for GetProjectsListQuery.
/// </summary>
public class GetProjectsListQueryHandler : IRequestHandler<GetProjectsListQuery, Result<List<ProjectDto>>>
{
    private readonly IApplicationDbContext _context;
    private readonly IUser _currentUser;
    private readonly ITenantService _tenantService;

    public GetProjectsListQueryHandler(
        IApplicationDbContext context,
        IUser currentUser,
        ITenantService tenantService)
    {
        _context = context;
        _currentUser = currentUser;
        _tenantService = tenantService;
    }

    public async Task<Result<List<ProjectDto>>> Handle(GetProjectsListQuery request, CancellationToken cancellationToken)
    {
        // Ensure user is authenticated
        if (string.IsNullOrEmpty(_currentUser.Id))
        {
            return Result<List<ProjectDto>>.Failure(new[] { "User must be authenticated." });
        }

        // Get current organization from tenant context
        var organizationId = _tenantService.GetCurrentTenantId();
        if (organizationId == null)
        {
            return Result<List<ProjectDto>>.Failure(new[] { "No workspace selected." });
        }

        // Base query
        var query = _context.Projects
            .Where(p => p.OrganizationId == organizationId.Value)
            .AsQueryable();

        // Apply filters
        if (request.Status.HasValue)
        {
            query = query.Where(p => p.Status == request.Status.Value);
        }

        if (!string.IsNullOrWhiteSpace(request.SearchTerm))
        {
            var searchTerm = request.SearchTerm.ToLower();
            query = query.Where(p =>
                p.Name.ToLower().Contains(searchTerm) ||
                (p.Description != null && p.Description.ToLower().Contains(searchTerm))
            );
        }

        // Filter by user membership if requested
        if (request.MyProjectsOnly)
        {
            query = query.Where(p =>
                p.ProjectMembers.Any(pm => pm.UserId == _currentUser.Id && pm.IsActive)
            );
        }

        // Exclude archived projects unless explicitly requested
        if (!request.IncludeArchived)
        {
            query = query.Where(p => p.Status != ProjectStatus.Archived);
        }

        // Execute query with projections
        var projects = await query
            .Select(p => new ProjectDto
            {
                Id = p.Id,
                Name = p.Name,
                Description = p.Description,
                Status = p.Status,
                TargetDate = p.TargetDate,
                CreatedAt = p.Created,
                LastModified = p.LastModified,
                DocumentCount = p.Documents.Count,
                FindingsCount = p.Findings.Count,
                FrameworkCount = p.ProjectFrameworks.Count,
                UserRole = p.ProjectMembers
                    .Where(pm => pm.UserId == _currentUser.Id && pm.IsActive)
                    .Select(pm => pm.Role.ToString())
                    .FirstOrDefault() ?? "Viewer"
            })
            .OrderByDescending(p => p.LastModified ?? p.CreatedAt)
            .ToListAsync(cancellationToken);

        return Result<List<ProjectDto>>.Success(projects);
    }
}
