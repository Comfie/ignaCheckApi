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
    private readonly IIdentityService _identityService;

    public GetWorkspaceMembersQueryHandler(
        IApplicationDbContext context,
        IUser currentUser,
        IIdentityService identityService)
    {
        _context = context;
        _currentUser = currentUser;
        _identityService = identityService;
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

        // Build query for OrganizationMembers
        var query = _context.OrganizationMembers
            .Where(m => m.OrganizationId == workspaceId.Value);

        // Apply role filter
        if (!string.IsNullOrEmpty(request.Role))
        {
            query = query.Where(m => m.Role == request.Role);
        }

        // Apply active status filter
        if (request.IsActive.HasValue)
        {
            query = query.Where(m => m.IsActive == request.IsActive.Value);
        }

        // Get all members (we'll filter by search after getting user details)
        var allMembers = await query.ToListAsync(cancellationToken);

        // Fetch user details for each member using IIdentityService
        var memberDtos = new List<WorkspaceMemberDto>();
        foreach (var member in allMembers)
        {
            var userObj = await _identityService.GetUserByIdAsync(member.UserId);
            if (userObj is Infrastructure.Identity.ApplicationUser user)
            {
                // Apply search filter if provided
                if (!string.IsNullOrEmpty(request.SearchQuery))
                {
                    var searchLower = request.SearchQuery.ToLower();
                    var matchesSearch =
                        (user.FirstName != null && user.FirstName.ToLower().Contains(searchLower)) ||
                        (user.LastName != null && user.LastName.ToLower().Contains(searchLower)) ||
                        (user.Email != null && user.Email.ToLower().Contains(searchLower));

                    if (!matchesSearch)
                    {
                        continue; // Skip this member
                    }
                }

                memberDtos.Add(new WorkspaceMemberDto
                {
                    UserId = member.UserId,
                    Email = user.Email ?? string.Empty,
                    FirstName = user.FirstName,
                    LastName = user.LastName,
                    FullName = (user.FirstName + " " + user.LastName).Trim(),
                    AvatarUrl = user.AvatarUrl,
                    JobTitle = user.JobTitle,
                    Role = member.Role,
                    JoinedDate = member.JoinedDate,
                    LastLoginDate = user.LastLoginDate,
                    IsActive = member.IsActive
                });
            }
        }

        // Apply sorting
        var sortBy = request.SortBy?.ToLower() ?? "name";
        var sortDirection = request.SortDirection?.ToLower() ?? "asc";

        memberDtos = sortBy switch
        {
            "email" => sortDirection == "desc"
                ? memberDtos.OrderByDescending(x => x.Email).ToList()
                : memberDtos.OrderBy(x => x.Email).ToList(),
            "joineddate" => sortDirection == "desc"
                ? memberDtos.OrderByDescending(x => x.JoinedDate).ToList()
                : memberDtos.OrderBy(x => x.JoinedDate).ToList(),
            "role" => sortDirection == "desc"
                ? memberDtos.OrderByDescending(x => x.Role).ToList()
                : memberDtos.OrderBy(x => x.Role).ToList(),
            _ => sortDirection == "desc"
                ? memberDtos.OrderByDescending(x => x.FirstName).ThenByDescending(x => x.LastName).ToList()
                : memberDtos.OrderBy(x => x.FirstName).ThenBy(x => x.LastName).ToList()
        };

        var totalCount = memberDtos.Count;

        // Apply pagination
        var pageNumber = Math.Max(1, request.PageNumber);
        var pageSize = Math.Min(100, Math.Max(1, request.PageSize));
        var skip = (pageNumber - 1) * pageSize;

        var members = memberDtos
            .Skip(skip)
            .Take(pageSize)
            .ToList();

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
