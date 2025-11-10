using IgnaCheck.Application.Common.Interfaces;
using IgnaCheck.Domain.Enums;

namespace IgnaCheck.Application.Findings.Queries.GetFindingsList;

/// <summary>
/// Query to get all compliance findings for a project with filtering and sorting.
/// </summary>
public record GetFindingsListQuery : IRequest<Result<List<FindingDto>>>
{
    /// <summary>
    /// Project ID to get findings for.
    /// </summary>
    public Guid ProjectId { get; init; }

    /// <summary>
    /// Filter by framework ID (optional).
    /// </summary>
    public Guid? FrameworkId { get; init; }

    /// <summary>
    /// Filter by compliance status (optional).
    /// </summary>
    public ComplianceStatus? Status { get; init; }

    /// <summary>
    /// Filter by workflow status (optional).
    /// </summary>
    public FindingWorkflowStatus? WorkflowStatus { get; init; }

    /// <summary>
    /// Filter by risk level (optional).
    /// </summary>
    public RiskLevel? RiskLevel { get; init; }

    /// <summary>
    /// Filter by assigned user (optional).
    /// </summary>
    public string? AssignedTo { get; init; }

    /// <summary>
    /// Search by title or description (optional).
    /// </summary>
    public string? SearchTerm { get; init; }

    /// <summary>
    /// Sort field (default: RiskLevel descending).
    /// </summary>
    public string SortBy { get; init; } = "RiskLevel";

    /// <summary>
    /// Sort direction (default: desc).
    /// </summary>
    public string SortDirection { get; init; } = "desc";
}

/// <summary>
/// Finding DTO for list display.
/// </summary>
public record FindingDto
{
    public Guid Id { get; init; }
    public string FindingCode { get; init; } = string.Empty;
    public string Title { get; init; } = string.Empty;
    public string Description { get; init; } = string.Empty;
    public ComplianceStatus Status { get; init; }
    public FindingWorkflowStatus WorkflowStatus { get; init; }
    public RiskLevel RiskLevel { get; init; }
    public string? AssignedTo { get; init; }
    public DateTime? DueDate { get; init; }
    public DateTime? ResolvedDate { get; init; }
    public decimal? ConfidenceScore { get; init; }
    public bool IsReviewed { get; init; }
    public int EvidenceCount { get; init; }
    public int CommentCount { get; init; }
    public DateTime CreatedAt { get; init; }

    // Control information
    public Guid ControlId { get; init; }
    public string ControlCode { get; init; } = string.Empty;
    public string ControlTitle { get; init; } = string.Empty;

    // Framework information
    public Guid FrameworkId { get; init; }
    public string FrameworkName { get; init; } = string.Empty;
    public string FrameworkCode { get; init; } = string.Empty;
}

/// <summary>
/// Handler for GetFindingsListQuery.
/// </summary>
public class GetFindingsListQueryHandler : IRequestHandler<GetFindingsListQuery, Result<List<FindingDto>>>
{
    private readonly IApplicationDbContext _context;
    private readonly IUser _currentUser;
    private readonly ITenantService _tenantService;

    public GetFindingsListQueryHandler(
        IApplicationDbContext context,
        IUser currentUser,
        ITenantService tenantService)
    {
        _context = context;
        _currentUser = currentUser;
        _tenantService = tenantService;
    }

    public async Task<Result<List<FindingDto>>> Handle(GetFindingsListQuery request, CancellationToken cancellationToken)
    {
        // Ensure user is authenticated
        if (string.IsNullOrEmpty(_currentUser.Id))
        {
            return Result<List<FindingDto>>.Failure(new[] { "User must be authenticated." });
        }

        // Get current organization
        var organizationId = _tenantService.GetCurrentTenantId();
        if (organizationId == null)
        {
            return Result<List<FindingDto>>.Failure(new[] { "No workspace selected." });
        }

        // Verify project exists and user has access
        var project = await _context.Projects
            .Include(p => p.ProjectMembers)
            .FirstOrDefaultAsync(p => p.Id == request.ProjectId, cancellationToken);

        if (project == null)
        {
            return Result<List<FindingDto>>.Failure(new[] { "Project not found." });
        }

        // Check if user is a member of the project
        var isMember = project.ProjectMembers.Any(pm => pm.UserId == _currentUser.Id && pm.IsActive);
        if (!isMember)
        {
            return Result<List<FindingDto>>.Failure(new[] { "Access denied. You are not a member of this project." });
        }

        // Base query
        var query = _context.ComplianceFindings
            .Include(f => f.Control)
            .ThenInclude(c => c.Framework)
            .Include(f => f.Evidence)
            .Include(f => f.Comments)
            .Where(f => f.ProjectId == request.ProjectId)
            .AsQueryable();

        // Apply filters
        if (request.FrameworkId.HasValue)
        {
            query = query.Where(f => f.Control.FrameworkId == request.FrameworkId.Value);
        }

        if (request.Status.HasValue)
        {
            query = query.Where(f => f.Status == request.Status.Value);
        }

        if (request.WorkflowStatus.HasValue)
        {
            query = query.Where(f => f.WorkflowStatus == request.WorkflowStatus.Value);
        }

        if (request.RiskLevel.HasValue)
        {
            query = query.Where(f => f.RiskLevel == request.RiskLevel.Value);
        }

        if (!string.IsNullOrWhiteSpace(request.AssignedTo))
        {
            query = query.Where(f => f.AssignedTo == request.AssignedTo);
        }

        if (!string.IsNullOrWhiteSpace(request.SearchTerm))
        {
            var searchTerm = request.SearchTerm.ToLower();
            query = query.Where(f =>
                f.Title.ToLower().Contains(searchTerm) ||
                f.Description.ToLower().Contains(searchTerm) ||
                f.FindingCode.ToLower().Contains(searchTerm)
            );
        }

        // Apply sorting
        query = request.SortBy.ToLower() switch
        {
            "risklevel" => request.SortDirection.ToLower() == "asc"
                ? query.OrderBy(f => f.RiskLevel)
                : query.OrderByDescending(f => f.RiskLevel),
            "status" => request.SortDirection.ToLower() == "asc"
                ? query.OrderBy(f => f.Status)
                : query.OrderByDescending(f => f.Status),
            "workflowstatus" => request.SortDirection.ToLower() == "asc"
                ? query.OrderBy(f => f.WorkflowStatus)
                : query.OrderByDescending(f => f.WorkflowStatus),
            "duedate" => request.SortDirection.ToLower() == "asc"
                ? query.OrderBy(f => f.DueDate ?? DateTime.MaxValue)
                : query.OrderByDescending(f => f.DueDate ?? DateTime.MinValue),
            "created" => request.SortDirection.ToLower() == "asc"
                ? query.OrderBy(f => f.Created)
                : query.OrderByDescending(f => f.Created),
            _ => query.OrderByDescending(f => f.RiskLevel).ThenByDescending(f => f.Created)
        };

        // Execute query with projections
        var findings = await query
            .Select(f => new FindingDto
            {
                Id = f.Id,
                FindingCode = f.FindingCode,
                Title = f.Title,
                Description = f.Description,
                Status = f.Status,
                WorkflowStatus = f.WorkflowStatus,
                RiskLevel = f.RiskLevel,
                AssignedTo = f.AssignedTo,
                DueDate = f.DueDate,
                ResolvedDate = f.ResolvedDate,
                ConfidenceScore = f.ConfidenceScore,
                IsReviewed = f.IsReviewed,
                EvidenceCount = f.Evidence.Count,
                CommentCount = f.Comments.Count,
                CreatedAt = f.Created,
                ControlId = f.ControlId,
                ControlCode = f.Control.ControlCode,
                ControlTitle = f.Control.Title,
                FrameworkId = f.Control.FrameworkId,
                FrameworkName = f.Control.Framework.Name,
                FrameworkCode = f.Control.Framework.Code
            })
            .ToListAsync(cancellationToken);

        return Result<List<FindingDto>>.Success(findings);
    }
}
