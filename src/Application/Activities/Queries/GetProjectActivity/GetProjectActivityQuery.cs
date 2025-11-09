using IgnaCheck.Application.Common.Interfaces;
using IgnaCheck.Domain.Entities;

namespace IgnaCheck.Application.Activities.Queries.GetProjectActivity;

/// <summary>
/// Query to get activity log for a project.
/// </summary>
public record GetProjectActivityQuery : IRequest<Result<List<ActivityLogDto>>>
{
    /// <summary>
    /// Project ID.
    /// </summary>
    public Guid ProjectId { get; init; }

    /// <summary>
    /// Filter by activity type (optional).
    /// </summary>
    public ActivityType? ActivityType { get; init; }

    /// <summary>
    /// Filter by user ID (optional).
    /// </summary>
    public string? UserId { get; init; }

    /// <summary>
    /// Start date for date range filter (optional).
    /// </summary>
    public DateTime? StartDate { get; init; }

    /// <summary>
    /// End date for date range filter (optional).
    /// </summary>
    public DateTime? EndDate { get; init; }

    /// <summary>
    /// Number of records to return (default: 100, max: 500).
    /// </summary>
    public int Limit { get; init; } = 100;
}

/// <summary>
/// Activity log DTO.
/// </summary>
public record ActivityLogDto
{
    public Guid Id { get; init; }
    public string UserName { get; init; } = string.Empty;
    public string UserEmail { get; init; } = string.Empty;
    public string ActivityType { get; init; } = string.Empty;
    public string EntityType { get; init; } = string.Empty;
    public Guid? EntityId { get; init; }
    public string? EntityName { get; init; }
    public string Description { get; init; } = string.Empty;
    public DateTime OccurredAt { get; init; }
    public string? Metadata { get; init; }
}

/// <summary>
/// Handler for GetProjectActivityQuery.
/// </summary>
public class GetProjectActivityQueryHandler : IRequestHandler<GetProjectActivityQuery, Result<List<ActivityLogDto>>>
{
    private readonly IApplicationDbContext _context;
    private readonly IUser _currentUser;
    private readonly ITenantService _tenantService;

    public GetProjectActivityQueryHandler(
        IApplicationDbContext context,
        IUser currentUser,
        ITenantService tenantService)
    {
        _context = context;
        _currentUser = currentUser;
        _tenantService = tenantService;
    }

    public async Task<Result<List<ActivityLogDto>>> Handle(GetProjectActivityQuery request, CancellationToken cancellationToken)
    {
        // Ensure user is authenticated
        if (string.IsNullOrEmpty(_currentUser.Id))
        {
            return Result<List<ActivityLogDto>>.Failure(new[] { "User must be authenticated." });
        }

        // Get current organization
        var organizationId = _tenantService.GetCurrentTenantId();
        if (organizationId == null)
        {
            return Result<List<ActivityLogDto>>.Failure(new[] { "No workspace selected." });
        }

        // Verify project exists and user has access
        var project = await _context.Projects
            .Include(p => p.ProjectMembers)
            .FirstOrDefaultAsync(p => p.Id == request.ProjectId && p.OrganizationId == organizationId.Value, cancellationToken);

        if (project == null)
        {
            return Result<List<ActivityLogDto>>.Failure(new[] { "Project not found." });
        }

        // Check if user has access to project
        var userMember = project.ProjectMembers.FirstOrDefault(pm => pm.UserId == _currentUser.Id && pm.IsActive);
        if (userMember == null)
        {
            return Result<List<ActivityLogDto>>.Failure(new[] { "You do not have access to this project." });
        }

        // Build query
        var query = _context.ActivityLogs
            .Where(al => al.ProjectId == request.ProjectId)
            .AsQueryable();

        // Apply filters
        if (request.ActivityType.HasValue)
        {
            query = query.Where(al => al.ActivityType == request.ActivityType.Value);
        }

        if (!string.IsNullOrWhiteSpace(request.UserId))
        {
            query = query.Where(al => al.UserId == request.UserId);
        }

        if (request.StartDate.HasValue)
        {
            query = query.Where(al => al.OccurredAt >= request.StartDate.Value);
        }

        if (request.EndDate.HasValue)
        {
            query = query.Where(al => al.OccurredAt <= request.EndDate.Value);
        }

        // Limit records (max 500)
        var limit = Math.Min(request.Limit, 500);

        // Execute query
        var activities = await query
            .OrderByDescending(al => al.OccurredAt)
            .Take(limit)
            .Select(al => new ActivityLogDto
            {
                Id = al.Id,
                UserName = al.UserName,
                UserEmail = al.UserEmail,
                ActivityType = al.ActivityType.ToString(),
                EntityType = al.EntityType,
                EntityId = al.EntityId,
                EntityName = al.EntityName,
                Description = al.Description,
                OccurredAt = al.OccurredAt,
                Metadata = al.Metadata
            })
            .ToListAsync(cancellationToken);

        return Result<List<ActivityLogDto>>.Success(activities);
    }
}
