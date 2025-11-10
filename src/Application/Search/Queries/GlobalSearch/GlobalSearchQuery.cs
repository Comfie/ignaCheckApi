using IgnaCheck.Application.Common.Interfaces;
using IgnaCheck.Domain.Entities;
using IgnaCheck.Domain.Enums;

namespace IgnaCheck.Application.Search.Queries.GlobalSearch;

/// <summary>
/// Query for global search across workspace.
/// </summary>
public record GlobalSearchQuery : IRequest<Result<GlobalSearchResultDto>>
{
    /// <summary>
    /// Search term.
    /// </summary>
    public string SearchTerm { get; init; } = string.Empty;

    /// <summary>
    /// Filter by result types (optional - if empty, searches all types).
    /// </summary>
    public List<SearchResultType>? ResultTypes { get; init; }

    /// <summary>
    /// Filter by project ID (optional).
    /// </summary>
    public Guid? ProjectId { get; init; }

    /// <summary>
    /// Maximum results per type (default: 10).
    /// </summary>
    public int MaxResultsPerType { get; init; } = 10;
}

public enum SearchResultType
{
    Project,
    Document,
    Finding,
    Task
}

/// <summary>
/// Global search results grouped by type.
/// </summary>
public record GlobalSearchResultDto
{
    public List<ProjectSearchResult> Projects { get; set; } = new();
    public List<DocumentSearchResult> Documents { get; set; } = new();
    public List<FindingSearchResult> Findings { get; set; } = new();
    public List<TaskSearchResult> Tasks { get; set; } = new();
    public int TotalResults { get; set; }
}

public record ProjectSearchResult
{
    public Guid Id { get; init; }
    public string Name { get; init; } = string.Empty;
    public string? Description { get; init; }
    public ProjectStatus Status { get; init; }
    public string Highlight { get; init; } = string.Empty;
}

public record DocumentSearchResult
{
    public Guid Id { get; init; }
    public Guid ProjectId { get; init; }
    public string ProjectName { get; init; } = string.Empty;
    public string FileName { get; init; } = string.Empty;
    public string? Description { get; init; }
    public string Highlight { get; init; } = string.Empty;
}

public record FindingSearchResult
{
    public Guid Id { get; init; }
    public Guid ProjectId { get; init; }
    public string ProjectName { get; init; } = string.Empty;
    public string Title { get; init; } = string.Empty;
    public RiskLevel RiskLevel { get; init; }
    public FindingWorkflowStatus WorkflowStatus { get; init; }
    public string FrameworkName { get; init; } = string.Empty;
    public string Highlight { get; init; } = string.Empty;
}

public record TaskSearchResult
{
    public Guid Id { get; init; }
    public Guid ProjectId { get; init; }
    public string ProjectName { get; init; } = string.Empty;
    public string Title { get; init; } = string.Empty;
    public TaskStatus Status { get; init; }
    public PriorityLevel Priority { get; init; }
    public string Highlight { get; init; } = string.Empty;
}

/// <summary>
/// Handler for GlobalSearchQuery.
/// </summary>
public class GlobalSearchQueryHandler : IRequestHandler<GlobalSearchQuery, Result<GlobalSearchResultDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly IUser _currentUser;
    private readonly ITenantService _tenantService;

    public GlobalSearchQueryHandler(
        IApplicationDbContext context,
        IUser currentUser,
        ITenantService tenantService)
    {
        _context = context;
        _currentUser = currentUser;
        _tenantService = tenantService;
    }

    public async Task<Result<GlobalSearchResultDto>> Handle(GlobalSearchQuery request, CancellationToken cancellationToken)
    {
        // Ensure user is authenticated
        if (string.IsNullOrEmpty(_currentUser.Id))
        {
            return Result<GlobalSearchResultDto>.Failure(new[] { "User must be authenticated." });
        }

        // Get current organization
        var organizationId = _tenantService.GetCurrentTenantId();
        if (organizationId == null)
        {
            return Result<GlobalSearchResultDto>.Failure(new[] { "No workspace selected." });
        }

        // Validate search term
        if (string.IsNullOrWhiteSpace(request.SearchTerm) || request.SearchTerm.Length < 2)
        {
            return Result<GlobalSearchResultDto>.Failure(new[] { "Search term must be at least 2 characters." });
        }

        var searchTerm = request.SearchTerm.ToLower();
        var maxResults = Math.Min(request.MaxResultsPerType, 50);
        var resultTypes = request.ResultTypes ?? Enum.GetValues<SearchResultType>().ToList();

        var result = new GlobalSearchResultDto();

        // Search Projects
        if (resultTypes.Contains(SearchResultType.Project))
        {
            var projectsQuery = _context.Projects
                .Include(p => p.ProjectMembers)
                .Where(p => p.OrganizationId == organizationId.Value &&
                           p.ProjectMembers.Any(pm => pm.UserId == _currentUser.Id && pm.IsActive));

            if (request.ProjectId.HasValue)
            {
                projectsQuery = projectsQuery.Where(p => p.Id == request.ProjectId.Value);
            }

            result.Projects = await projectsQuery
                .Where(p => p.Name.ToLower().Contains(searchTerm) ||
                           (p.Description != null && p.Description.ToLower().Contains(searchTerm)))
                .Take(maxResults)
                .Select(p => new ProjectSearchResult
                {
                    Id = p.Id,
                    Name = p.Name,
                    Description = p.Description,
                    Status = p.Status,
                    Highlight = p.Name.ToLower().Contains(searchTerm) ? p.Name : p.Description ?? ""
                })
                .ToListAsync(cancellationToken);
        }

        // Search Documents
        if (resultTypes.Contains(SearchResultType.Document))
        {
            var documentsQuery = _context.Documents
                .Include(d => d.Project)
                .ThenInclude(p => p.ProjectMembers)
                .Where(d => d.Project.OrganizationId == organizationId.Value &&
                           d.Project.ProjectMembers.Any(pm => pm.UserId == _currentUser.Id && pm.IsActive));

            if (request.ProjectId.HasValue)
            {
                documentsQuery = documentsQuery.Where(d => d.ProjectId == request.ProjectId.Value);
            }

            result.Documents = await documentsQuery
                .Where(d => d.FileName.ToLower().Contains(searchTerm) ||
                           (d.Description != null && d.Description.ToLower().Contains(searchTerm)))
                .Take(maxResults)
                .Select(d => new DocumentSearchResult
                {
                    Id = d.Id,
                    ProjectId = d.ProjectId,
                    ProjectName = d.Project.Name,
                    FileName = d.FileName,
                    Description = d.Description,
                    Highlight = d.FileName.ToLower().Contains(searchTerm) ? d.FileName : d.Description ?? ""
                })
                .ToListAsync(cancellationToken);
        }

        // Search Findings
        if (resultTypes.Contains(SearchResultType.Finding))
        {
            var findingsQuery = _context.ComplianceFindings
                .Include(f => f.Project)
                .ThenInclude(p => p.ProjectMembers)
                .Include(f => f.Control)
                .ThenInclude(c => c.Framework)
                .Where(f => f.Project.OrganizationId == organizationId.Value &&
                           f.Project.ProjectMembers.Any(pm => pm.UserId == _currentUser.Id && pm.IsActive));

            if (request.ProjectId.HasValue)
            {
                findingsQuery = findingsQuery.Where(f => f.ProjectId == request.ProjectId.Value);
            }

            result.Findings = await findingsQuery
                .Where(f => f.Title.ToLower().Contains(searchTerm) ||
                           f.Description.ToLower().Contains(searchTerm) ||
                           f.FindingCode.ToLower().Contains(searchTerm))
                .Take(maxResults)
                .Select(f => new FindingSearchResult
                {
                    Id = f.Id,
                    ProjectId = f.ProjectId,
                    ProjectName = f.Project.Name,
                    Title = f.Title,
                    RiskLevel = f.RiskLevel,
                    WorkflowStatus = f.WorkflowStatus,
                    FrameworkName = f.Control.Framework.Name,
                    Highlight = f.Title.ToLower().Contains(searchTerm) ? f.Title :
                               (f.Description.Length > 100 ? f.Description.Substring(0, 100) + "..." : f.Description)
                })
                .ToListAsync(cancellationToken);
        }

        // Search Tasks
        if (resultTypes.Contains(SearchResultType.Task))
        {
            var tasksQuery = _context.RemediationTasks
                .Include(t => t.Project)
                .ThenInclude(p => p.ProjectMembers)
                .Where(t => t.Project.OrganizationId == organizationId.Value &&
                           t.Project.ProjectMembers.Any(pm => pm.UserId == _currentUser.Id && pm.IsActive));

            if (request.ProjectId.HasValue)
            {
                tasksQuery = tasksQuery.Where(t => t.ProjectId == request.ProjectId.Value);
            }

            result.Tasks = await tasksQuery
                .Where(t => t.Title.ToLower().Contains(searchTerm) ||
                           t.Description.ToLower().Contains(searchTerm))
                .Take(maxResults)
                .Select(t => new TaskSearchResult
                {
                    Id = t.Id,
                    ProjectId = t.ProjectId,
                    ProjectName = t.Project.Name,
                    Title = t.Title,
                    Status = t.Status,
                    Priority = t.Priority,
                    Highlight = t.Title.ToLower().Contains(searchTerm) ? t.Title :
                               (t.Description.Length > 100 ? t.Description.Substring(0, 100) + "..." : t.Description)
                })
                .ToListAsync(cancellationToken);
        }

        result = result with
        {
            TotalResults = result.Projects.Count + result.Documents.Count +
                          result.Findings.Count + result.Tasks.Count
        };

        return Result<GlobalSearchResultDto>.Success(result);
    }
}
