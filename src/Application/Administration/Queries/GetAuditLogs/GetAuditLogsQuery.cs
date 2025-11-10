using IgnaCheck.Application.Common.Interfaces;
using IgnaCheck.Domain.Constants;
using IgnaCheck.Domain.Entities;
using IgnaCheck.Domain.Enums;

namespace IgnaCheck.Application.Administration.Queries.GetAuditLogs;

/// <summary>
/// Query to get workspace-level audit logs with filtering.
/// Only accessible by workspace owners and admins.
/// </summary>
public record GetAuditLogsQuery : IRequest<Result<AuditLogsResultDto>>
{
    /// <summary>
    /// Filter by activity type (optional).
    /// </summary>
    public ActivityType? ActivityType { get; init; }

    /// <summary>
    /// Filter by user ID (optional).
    /// </summary>
    public string? UserId { get; init; }

    /// <summary>
    /// Filter by entity type (e.g., "Project", "Finding", "Document") (optional).
    /// </summary>
    public string? EntityType { get; init; }

    /// <summary>
    /// Filter by entity ID (optional).
    /// </summary>
    public Guid? EntityId { get; init; }

    /// <summary>
    /// Start date for date range filter (optional).
    /// </summary>
    public DateTime? StartDate { get; init; }

    /// <summary>
    /// End date for date range filter (optional).
    /// </summary>
    public DateTime? EndDate { get; init; }

    /// <summary>
    /// Search term for activity description (optional).
    /// </summary>
    public string? SearchTerm { get; init; }

    /// <summary>
    /// Number of records to return (default: 100, max: 1000).
    /// </summary>
    public int Limit { get; init; } = 100;

    /// <summary>
    /// Offset for pagination (default: 0).
    /// </summary>
    public int Offset { get; init; } = 0;
}

/// <summary>
/// Audit logs result with pagination info.
/// </summary>
public record AuditLogsResultDto
{
    public List<AuditLogEntryDto> Logs { get; init; } = new();
    public int TotalCount { get; init; }
    public int ReturnedCount { get; init; }
    public int Offset { get; init; }
}

/// <summary>
/// Individual audit log entry.
/// </summary>
public record AuditLogEntryDto
{
    public Guid Id { get; init; }
    public ActivityType ActivityType { get; init; }
    public string Description { get; init; } = string.Empty;
    public string UserId { get; init; } = string.Empty;
    public string UserName { get; init; } = string.Empty;
    public string? EntityType { get; init; }
    public Guid? EntityId { get; init; }
    public string? EntityName { get; init; }
    public string? Metadata { get; init; }
    public DateTime Timestamp { get; init; }
}

/// <summary>
/// Handler for GetAuditLogsQuery.
/// </summary>
public class GetAuditLogsQueryHandler : IRequestHandler<GetAuditLogsQuery, Result<AuditLogsResultDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly IUser _currentUser;
    private readonly ITenantService _tenantService;

    public GetAuditLogsQueryHandler(
        IApplicationDbContext context,
        IUser currentUser,
        ITenantService tenantService)
    {
        _context = context;
        _currentUser = currentUser;
        _tenantService = tenantService;
    }

    public async Task<Result<AuditLogsResultDto>> Handle(GetAuditLogsQuery request, CancellationToken cancellationToken)
    {
        // Ensure user is authenticated
        if (string.IsNullOrEmpty(_currentUser.Id))
        {
            return Result<AuditLogsResultDto>.Failure(new[] { "User must be authenticated." });
        }

        // Get current organization
        var organizationId = _tenantService.GetCurrentTenantId();
        if (organizationId == null)
        {
            return Result<AuditLogsResultDto>.Failure(new[] { "No workspace selected." });
        }

        // Verify user is an owner or admin
        var member = await _context.OrganizationMembers
            .FirstOrDefaultAsync(m => m.OrganizationId == organizationId.Value && m.UserId == _currentUser.Id, cancellationToken);

        if (member == null)
        {
            return Result<AuditLogsResultDto>.Failure(new[] { "Access denied. You are not a member of this workspace." });
        }

        if (member.Role != WorkspaceRoles.Owner && member.Role != WorkspaceRoles.Admin)
        {
            return Result<AuditLogsResultDto>.Failure(new[] { "Access denied. Only workspace owners and admins can view audit logs." });
        }

        // Validate limit
        var limit = Math.Min(request.Limit, 1000);

        // Build base query
        var query = _context.ActivityLogs
            .Where(a => a.OrganizationId == organizationId.Value)
            .AsQueryable();

        // Apply filters
        if (request.ActivityType.HasValue)
        {
            query = query.Where(a => a.ActivityType == request.ActivityType.Value);
        }

        if (!string.IsNullOrWhiteSpace(request.UserId))
        {
            query = query.Where(a => a.UserId == request.UserId);
        }

        if (!string.IsNullOrWhiteSpace(request.EntityType))
        {
            query = query.Where(a => a.EntityType == request.EntityType);
        }

        if (request.EntityId.HasValue)
        {
            query = query.Where(a => a.EntityId == request.EntityId.Value);
        }

        if (request.StartDate.HasValue)
        {
            query = query.Where(a => a.OccurredAt >= request.StartDate.Value);
        }

        if (request.EndDate.HasValue)
        {
            // Include the entire end date
            var endOfDay = request.EndDate.Value.Date.AddDays(1).AddTicks(-1);
            query = query.Where(a => a.OccurredAt <= endOfDay);
        }

        if (!string.IsNullOrWhiteSpace(request.SearchTerm))
        {
            var searchTerm = request.SearchTerm.ToLower();
            query = query.Where(a =>
                a.Description.ToLower().Contains(searchTerm) ||
                (a.EntityName != null && a.EntityName.ToLower().Contains(searchTerm))
            );
        }

        // Get total count before pagination
        var totalCount = await query.CountAsync(cancellationToken);

        // Apply pagination and ordering
        var logs = await query
            .OrderByDescending(a => a.OccurredAt)
            .Skip(request.Offset)
            .Take(limit)
            .Select(a => new AuditLogEntryDto
            {
                Id = a.Id,
                ActivityType = a.ActivityType,
                Description = a.Description,
                UserId = a.UserId,
                UserName = a.UserName ?? "Unknown User",
                EntityType = a.EntityType,
                EntityId = a.EntityId,
                EntityName = a.EntityName,
                Metadata = a.Metadata,
                Timestamp = a.OccurredAt
            })
            .ToListAsync(cancellationToken);

        var result = new AuditLogsResultDto
        {
            Logs = logs,
            TotalCount = totalCount,
            ReturnedCount = logs.Count,
            Offset = request.Offset
        };

        return Result<AuditLogsResultDto>.Success(result);
    }
}
