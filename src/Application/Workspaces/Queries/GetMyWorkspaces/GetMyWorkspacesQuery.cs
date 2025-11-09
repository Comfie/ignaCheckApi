using IgnaCheck.Application.Common.Interfaces;

namespace IgnaCheck.Application.Workspaces.Queries.GetMyWorkspaces;

/// <summary>
/// Query to get all workspaces the current user belongs to.
/// </summary>
public record GetMyWorkspacesQuery : IRequest<Result<List<WorkspaceDto>>>
{
}

/// <summary>
/// Workspace DTO.
/// </summary>
public record WorkspaceDto
{
    /// <summary>
    /// Workspace ID.
    /// </summary>
    public Guid Id { get; init; }

    /// <summary>
    /// Workspace name.
    /// </summary>
    public string Name { get; init; } = string.Empty;

    /// <summary>
    /// Workspace description.
    /// </summary>
    public string? Description { get; init; }

    /// <summary>
    /// Workspace slug (URL identifier).
    /// </summary>
    public string? Slug { get; init; }

    /// <summary>
    /// Workspace logo URL.
    /// </summary>
    public string? LogoUrl { get; init; }

    /// <summary>
    /// Subscription tier.
    /// </summary>
    public string? SubscriptionTier { get; init; }

    /// <summary>
    /// Trial end date (if applicable).
    /// </summary>
    public DateTime? TrialEndsAt { get; init; }

    /// <summary>
    /// Whether the workspace is active.
    /// </summary>
    public bool IsActive { get; init; }

    /// <summary>
    /// Current user's role in the workspace.
    /// </summary>
    public string Role { get; init; } = string.Empty;

    /// <summary>
    /// Date the user joined the workspace.
    /// </summary>
    public DateTime JoinedDate { get; init; }

    /// <summary>
    /// Number of members in the workspace.
    /// </summary>
    public int MemberCount { get; init; }

    /// <summary>
    /// Maximum members allowed (null = unlimited).
    /// </summary>
    public int? MaxMembers { get; init; }

    /// <summary>
    /// Maximum projects allowed (null = unlimited).
    /// </summary>
    public int? MaxProjects { get; init; }
}

/// <summary>
/// Handler for the GetMyWorkspacesQuery.
/// </summary>
public class GetMyWorkspacesQueryHandler : IRequestHandler<GetMyWorkspacesQuery, Result<List<WorkspaceDto>>>
{
    private readonly IApplicationDbContext _context;
    private readonly IUser _currentUser;

    public GetMyWorkspacesQueryHandler(
        IApplicationDbContext context,
        IUser currentUser)
    {
        _context = context;
        _currentUser = currentUser;
    }

    public async Task<Result<List<WorkspaceDto>>> Handle(GetMyWorkspacesQuery request, CancellationToken cancellationToken)
    {
        // Ensure user is authenticated
        if (string.IsNullOrEmpty(_currentUser.Id))
        {
            return Result<List<WorkspaceDto>>.Failure(new[] { "User must be authenticated." });
        }

        // Get all workspaces the user belongs to
        var workspaces = await _context.OrganizationMembers
            .Where(m => m.UserId == _currentUser.Id && m.IsActive)
            .Include(m => m.Organization)
            .Select(m => new WorkspaceDto
            {
                Id = m.Organization.Id,
                Name = m.Organization.Name,
                Description = m.Organization.Description,
                Slug = m.Organization.Slug,
                LogoUrl = m.Organization.LogoUrl,
                SubscriptionTier = m.Organization.SubscriptionTier,
                TrialEndsAt = m.Organization.TrialEndsAt,
                IsActive = m.Organization.IsActive,
                Role = m.Role,
                JoinedDate = m.JoinedDate,
                MemberCount = m.Organization.Members.Count(mem => mem.IsActive),
                MaxMembers = m.Organization.MaxMembers,
                MaxProjects = m.Organization.MaxProjects
            })
            .OrderBy(w => w.Name)
            .ToListAsync(cancellationToken);

        return Result<List<WorkspaceDto>>.Success(workspaces);
    }
}
