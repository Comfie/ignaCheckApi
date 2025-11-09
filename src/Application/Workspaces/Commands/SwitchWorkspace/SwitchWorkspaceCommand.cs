using IgnaCheck.Application.Common.Interfaces;

namespace IgnaCheck.Application.Workspaces.Commands.SwitchWorkspace;

/// <summary>
/// Command to switch the current workspace context.
/// </summary>
public record SwitchWorkspaceCommand : IRequest<Result<SwitchWorkspaceResponse>>
{
    /// <summary>
    /// Target workspace ID.
    /// </summary>
    public Guid WorkspaceId { get; init; }
}

/// <summary>
/// Response for successful workspace switch.
/// </summary>
public record SwitchWorkspaceResponse
{
    /// <summary>
    /// JWT access token with updated organization context.
    /// </summary>
    public string AccessToken { get; init; } = string.Empty;

    /// <summary>
    /// Token type (usually "Bearer").
    /// </summary>
    public string TokenType { get; init; } = "Bearer";

    /// <summary>
    /// Token expiration in seconds.
    /// </summary>
    public int ExpiresIn { get; init; }

    /// <summary>
    /// Workspace ID.
    /// </summary>
    public Guid WorkspaceId { get; init; }

    /// <summary>
    /// Workspace name.
    /// </summary>
    public string WorkspaceName { get; init; } = string.Empty;

    /// <summary>
    /// User's role in the workspace.
    /// </summary>
    public string WorkspaceRole { get; init; } = string.Empty;
}

/// <summary>
/// Handler for the SwitchWorkspaceCommand.
/// </summary>
public class SwitchWorkspaceCommandHandler : IRequestHandler<SwitchWorkspaceCommand, Result<SwitchWorkspaceResponse>>
{
    private readonly IApplicationDbContext _context;
    private readonly IUser _currentUser;
    private readonly IJwtTokenGenerator _jwtTokenGenerator;
    private readonly IIdentityService _identityService;

    public SwitchWorkspaceCommandHandler(
        IApplicationDbContext context,
        IUser currentUser,
        IJwtTokenGenerator jwtTokenGenerator,
        IIdentityService identityService)
    {
        _context = context;
        _currentUser = currentUser;
        _jwtTokenGenerator = jwtTokenGenerator;
        _identityService = identityService;
    }

    public async Task<Result<SwitchWorkspaceResponse>> Handle(SwitchWorkspaceCommand request, CancellationToken cancellationToken)
    {
        // Ensure user is authenticated
        if (string.IsNullOrEmpty(_currentUser.Id))
        {
            return Result<SwitchWorkspaceResponse>.Failure(new[] { "User must be authenticated to switch workspace." });
        }

        // Get user details
        var user = await _identityService.GetUserByIdAsync(_currentUser.Id);
        if (user is not IgnaCheck.Infrastructure.Identity.ApplicationUser appUser)
        {
            return Result<SwitchWorkspaceResponse>.Failure(new[] { "User not found." });
        }

        // Check if workspace exists
        var organization = await _context.Organizations
            .FirstOrDefaultAsync(o => o.Id == request.WorkspaceId, cancellationToken);

        if (organization == null)
        {
            return Result<SwitchWorkspaceResponse>.Failure(new[] { "Workspace not found." });
        }

        // Check if user is a member of this workspace
        var membership = await _context.OrganizationMembers
            .FirstOrDefaultAsync(m => m.OrganizationId == request.WorkspaceId && m.UserId == _currentUser.Id && m.IsActive, cancellationToken);

        if (membership == null)
        {
            return Result<SwitchWorkspaceResponse>.Failure(new[] { "You are not a member of this workspace." });
        }

        // Get user roles for JWT
        var roles = new List<string>(); // TODO: Load actual user roles

        // Generate new JWT token with organization context
        var expiresInMinutes = 60; // 1 hour
        var accessToken = _jwtTokenGenerator.GenerateAccessToken(
            userId: _currentUser.Id,
            email: appUser.Email!,
            firstName: appUser.FirstName,
            lastName: appUser.LastName,
            roles: roles,
            organizationId: organization.Id,
            organizationRole: membership.Role,
            expiresInMinutes: expiresInMinutes
        );

        var response = new SwitchWorkspaceResponse
        {
            AccessToken = accessToken,
            TokenType = "Bearer",
            ExpiresIn = expiresInMinutes * 60, // Convert to seconds
            WorkspaceId = organization.Id,
            WorkspaceName = organization.Name,
            WorkspaceRole = membership.Role
        };

        return Result<SwitchWorkspaceResponse>.Success(response);
    }
}
