using IgnaCheck.Application.Common.Interfaces;

namespace IgnaCheck.Application.Users.Queries.GetWorkspaceMembers;

/// <summary>
/// Query to get workspace members with filtering and pagination.
/// </summary>
public record GetWorkspaceMembersQuery : IRequest<Result<WorkspaceMembersResponse>>
{
    /// <summary>
    /// Workspace ID (optional - uses current workspace from JWT if not provided).
    /// </summary>
    public Guid? WorkspaceId { get; init; }

    /// <summary>
    /// Search query (searches by name or email).
    /// </summary>
    public string? SearchQuery { get; init; }

    /// <summary>
    /// Filter by role.
    /// </summary>
    public string? Role { get; init; }

    /// <summary>
    /// Filter by active status.
    /// </summary>
    public bool? IsActive { get; init; }

    /// <summary>
    /// Page number (1-based).
    /// </summary>
    public int PageNumber { get; init; } = 1;

    /// <summary>
    /// Page size (max 100).
    /// </summary>
    public int PageSize { get; init; } = 20;

    /// <summary>
    /// Sort by field (Name, Email, JoinedDate, Role).
    /// </summary>
    public string? SortBy { get; init; }

    /// <summary>
    /// Sort direction (Asc or Desc).
    /// </summary>
    public string? SortDirection { get; init; }
}

/// <summary>
/// Workspace members response with pagination.
/// </summary>
public record WorkspaceMembersResponse
{
    public List<WorkspaceMemberDto> Members { get; init; } = new();
    public int TotalCount { get; init; }
    public int PageNumber { get; init; }
    public int PageSize { get; init; }
    public int TotalPages { get; init; }
}

/// <summary>
/// Workspace member DTO.
/// </summary>
public record WorkspaceMemberDto
{
    public string UserId { get; init; } = string.Empty;
    public string Email { get; init; } = string.Empty;
    public string? FirstName { get; init; }
    public string? LastName { get; init; }
    public string FullName { get; init; } = string.Empty;
    public string? AvatarUrl { get; init; }
    public string? JobTitle { get; init; }
    public string Role { get; init; } = string.Empty;
    public DateTime JoinedDate { get; init; }
    public DateTime? LastLoginDate { get; init; }
    public bool IsActive { get; init; }
}

/// <summary>
/// Handler for the GetWorkspaceMembersQuery.
/// </summary>
public class GetWorkspaceMembersQueryHandler : IRequestHandler<GetWorkspaceMembersQuery, Result<WorkspaceMembersResponse>>
{
    private readonly IApplicationDbContext _context;
    private readonly IUser _currentUser;

    public GetWorkspaceMembersQueryHandler(
        IApplicationDbContext context,
        IUser currentUser)
    {
        _context = context;
        _currentUser = currentUser;
    }

    public async Task<Result<WorkspaceMembersResponse>> Handle(GetWorkspaceMembersQuery request, CancellationToken cancellationToken)
    {
        // Ensure user is authenticated
        if (string.IsNullOrEmpty(_currentUser.Id))
        {
            return Result<WorkspaceMembersResponse>.Failure(new[] { "User must be authenticated." });
        }

        // Determine workspace ID
        var workspaceId = request.WorkspaceId ?? _currentUser.OrganizationId;
        if (!workspaceId.HasValue)
        {
            return Result<WorkspaceMembersResponse>.Failure(new[] { "Workspace ID is required." });
        }

        // Check if current user is a member of the workspace
        var currentUserMembership = await _context.OrganizationMembers
            .FirstOrDefaultAsync(m => m.OrganizationId == workspaceId.Value && m.UserId == _currentUser.Id && m.IsActive, cancellationToken);

        if (currentUserMembership == null)
        {
            return Result<WorkspaceMembersResponse>.Failure(new[] { "You are not a member of this workspace." });
        }

        // Build query
        var query = _context.OrganizationMembers
            .Where(m => m.OrganizationId == workspaceId.Value)
            .Include(m => m.User);

        // Apply filters
        if (!string.IsNullOrEmpty(request.SearchQuery))
        {
            var searchLower = request.SearchQuery.ToLower();
            query = query.Where(m =>
                (m.User.FirstName != null && m.User.FirstName.ToLower().Contains(searchLower)) ||
                (m.User.LastName != null && m.User.LastName.ToLower().Contains(searchLower)) ||
                (m.User.Email != null && m.User.Email.ToLower().Contains(searchLower))
            );
        }

        if (!string.IsNullOrEmpty(request.Role))
        {
            query = query.Where(m => m.Role == request.Role);
        }

        if (request.IsActive.HasValue)
        {
            query = query.Where(m => m.IsActive == request.IsActive.Value);
        }

        // Get total count before pagination
        var totalCount = await query.CountAsync(cancellationToken);

        // Apply sorting
        var sortBy = request.SortBy?.ToLower() ?? "name";
        var sortDirection = request.SortDirection?.ToLower() ?? "asc";

        query = sortBy switch
        {
            "email" => sortDirection == "desc"
                ? query.OrderByDescending(m => m.User.Email)
                : query.OrderBy(m => m.User.Email),
            "joineddate" => sortDirection == "desc"
                ? query.OrderByDescending(m => m.JoinedDate)
                : query.OrderBy(m => m.JoinedDate),
            "role" => sortDirection == "desc"
                ? query.OrderByDescending(m => m.Role)
                : query.OrderBy(m => m.Role),
            _ => sortDirection == "desc"
                ? query.OrderByDescending(m => m.User.FirstName).ThenByDescending(m => m.User.LastName)
                : query.OrderBy(m => m.User.FirstName).ThenBy(m => m.User.LastName)
        };

        // Apply pagination
        var pageNumber = Math.Max(1, request.PageNumber);
        var pageSize = Math.Min(100, Math.Max(1, request.PageSize));
        var skip = (pageNumber - 1) * pageSize;

        var members = await query
            .Skip(skip)
            .Take(pageSize)
            .Select(m => new WorkspaceMemberDto
            {
                UserId = m.UserId,
                Email = m.User.Email ?? string.Empty,
                FirstName = m.User.FirstName,
                LastName = m.User.LastName,
                FullName = (m.User.FirstName + " " + m.User.LastName).Trim(),
                AvatarUrl = m.User.AvatarUrl,
                JobTitle = m.User.JobTitle,
                Role = m.Role,
                JoinedDate = m.JoinedDate,
                LastLoginDate = m.User.LastLoginDate,
                IsActive = m.IsActive
            })
            .ToListAsync(cancellationToken);

        var totalPages = (int)Math.Ceiling(totalCount / (double)pageSize);

        var response = new WorkspaceMembersResponse
        {
            Members = members,
            TotalCount = totalCount,
            PageNumber = pageNumber,
            PageSize = pageSize,
            TotalPages = totalPages
        };

        return Result<WorkspaceMembersResponse>.Success(response);
    }
}
