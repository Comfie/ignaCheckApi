using IgnaCheck.Application.Common.Interfaces;

namespace IgnaCheck.Application.Users.Queries.GetWorkspaceInvitations;

/// <summary>
/// Query to get all invitations for a workspace.
/// </summary>
public record GetWorkspaceInvitationsQuery : IRequest<Result<List<InvitationDto>>>
{
    /// <summary>
    /// Workspace ID (optional - uses current workspace from JWT if not provided).
    /// </summary>
    public Guid? WorkspaceId { get; init; }

    /// <summary>
    /// Filter by status (optional - returns all if not provided).
    /// </summary>
    public string? Status { get; init; }
}

/// <summary>
/// Invitation DTO.
/// </summary>
public record InvitationDto
{
    public Guid Id { get; init; }
    public string Email { get; init; } = string.Empty;
    public string Role { get; init; } = string.Empty;
    public string Status { get; init; } = string.Empty;
    public DateTime CreatedDate { get; init; }
    public DateTime ExpiresDate { get; init; }
    public DateTime? AcceptedDate { get; init; }
    public string? InvitedByUserName { get; init; }
    public string? Message { get; init; }
}

/// <summary>
/// Handler for the GetWorkspaceInvitationsQuery.
/// </summary>
public class GetWorkspaceInvitationsQueryHandler : IRequestHandler<GetWorkspaceInvitationsQuery, Result<List<InvitationDto>>>
{
    private readonly IApplicationDbContext _context;
    private readonly IUser _currentUser;

    public GetWorkspaceInvitationsQueryHandler(
        IApplicationDbContext context,
        IUser currentUser)
    {
        _context = context;
        _currentUser = currentUser;
    }

    public async Task<Result<List<InvitationDto>>> Handle(GetWorkspaceInvitationsQuery request, CancellationToken cancellationToken)
    {
        // Ensure user is authenticated
        if (string.IsNullOrEmpty(_currentUser.Id))
        {
            return Result<List<InvitationDto>>.Failure(new[] { "User must be authenticated." });
        }

        // Determine workspace ID
        var workspaceId = request.WorkspaceId ?? _currentUser.OrganizationId;
        if (!workspaceId.HasValue)
        {
            return Result<List<InvitationDto>>.Failure(new[] { "Workspace ID is required." });
        }

        // Check if current user is a member of the workspace
        var currentUserMembership = await _context.OrganizationMembers
            .FirstOrDefaultAsync(m => m.OrganizationId == workspaceId.Value && m.UserId == _currentUser.Id && m.IsActive, cancellationToken);

        if (currentUserMembership == null)
        {
            return Result<List<InvitationDto>>.Failure(new[] { "You are not a member of this workspace." });
        }

        // Only Owners and Admins can view invitations
        if (currentUserMembership.Role != Domain.Constants.WorkspaceRoles.Owner && currentUserMembership.Role != Domain.Constants.WorkspaceRoles.Admin)
        {
            return Result<List<InvitationDto>>.Failure(new[] { "Only workspace Owners and Admins can view invitations." });
        }

        // Build query
        var query = _context.Invitations
            .Where(i => i.OrganizationId == workspaceId.Value);

        // Apply status filter if provided
        if (!string.IsNullOrEmpty(request.Status))
        {
            query = query.Where(i => i.Status.ToString() == request.Status);
        }

        // Get invitations with inviter details
        var invitations = await query
            .OrderByDescending(i => i.Created)
            .Select(i => new InvitationDto
            {
                Id = i.Id,
                Email = i.Email,
                Role = i.Role,
                Status = i.Status.ToString(),
                CreatedDate = i.Created,
                ExpiresDate = i.ExpiresDate,
                AcceptedDate = i.AcceptedDate,
                InvitedByUserName = i.InvitedByUserId != null
                    ? _context.Users
                        .Where(u => u.Id == i.InvitedByUserId)
                        .Select(u => u.FirstName + " " + u.LastName)
                        .FirstOrDefault()
                    : null,
                Message = i.Message
            })
            .ToListAsync(cancellationToken);

        return Result<List<InvitationDto>>.Success(invitations);
    }
}
