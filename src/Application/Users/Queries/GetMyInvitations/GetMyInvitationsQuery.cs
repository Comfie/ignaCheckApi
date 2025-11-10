using IgnaCheck.Application.Common.Interfaces;
using IgnaCheck.Domain.Enums;

namespace IgnaCheck.Application.Users.Queries.GetMyInvitations;

/// <summary>
/// Query to get all pending invitations for the current user.
/// </summary>
public record GetMyInvitationsQuery : IRequest<Result<List<MyInvitationDto>>>
{
}

/// <summary>
/// My invitation DTO.
/// </summary>
public record MyInvitationDto
{
    public string Token { get; init; } = string.Empty;
    public Guid WorkspaceId { get; init; }
    public string WorkspaceName { get; init; } = string.Empty;
    public string Role { get; init; } = string.Empty;
    public DateTime CreatedDate { get; init; }
    public DateTime ExpiresDate { get; init; }
    public string? InvitedByUserName { get; init; }
    public string? Message { get; init; }
}

/// <summary>
/// Handler for the GetMyInvitationsQuery.
/// </summary>
public class GetMyInvitationsQueryHandler : IRequestHandler<GetMyInvitationsQuery, Result<List<MyInvitationDto>>>
{
    private readonly IApplicationDbContext _context;
    private readonly IUser _currentUser;
    private readonly IIdentityService _identityService;

    public GetMyInvitationsQueryHandler(
        IApplicationDbContext context,
        IUser currentUser,
        IIdentityService identityService)
    {
        _context = context;
        _currentUser = currentUser;
        _identityService = identityService;
    }

    public async Task<Result<List<MyInvitationDto>>> Handle(GetMyInvitationsQuery request, CancellationToken cancellationToken)
    {
        // Ensure user is authenticated
        if (string.IsNullOrEmpty(_currentUser.Id))
        {
            return Result<List<MyInvitationDto>>.Failure(new[] { "User must be authenticated." });
        }

        // Get user details
        var user = await _identityService.GetUserByIdAsync(_currentUser.Id);
        if (user is not IgnaCheck.Infrastructure.Identity.ApplicationUser appUser || string.IsNullOrEmpty(appUser.Email))
        {
            return Result<List<MyInvitationDto>>.Failure(new[] { "User not found." });
        }

        // Get all pending invitations for this user's email
        var invitations = await _context.Invitations
            .Where(i => i.Email.ToLower() == appUser.Email.ToLower() &&
                       i.Status == InvitationStatus.Pending &&
                       i.ExpiresDate > DateTime.UtcNow)
            .Include(i => i.Organization)
            .OrderByDescending(i => i.Created)
            .ToListAsync(cancellationToken);

        // Map to DTOs and fetch inviter names using IIdentityService
        var invitationDtos = new List<MyInvitationDto>();
        foreach (var invitation in invitations)
        {
            string? invitedByUserName = null;
            if (!string.IsNullOrEmpty(invitation.InvitedBy))
            {
                var inviterUser = await _identityService.GetUserByIdAsync(invitation.InvitedBy);
                if (inviterUser is IgnaCheck.Infrastructure.Identity.ApplicationUser inviterAppUser)
                {
                    invitedByUserName = $"{inviterAppUser.FirstName} {inviterAppUser.LastName}".Trim();
                }
            }

            invitationDtos.Add(new MyInvitationDto
            {
                Token = invitation.Token,
                WorkspaceId = invitation.OrganizationId,
                WorkspaceName = invitation.Organization.Name,
                Role = invitation.Role,
                CreatedDate = invitation.Created,
                ExpiresDate = invitation.ExpiresDate,
                InvitedByUserName = invitedByUserName,
                Message = invitation.Message
            });
        }

        return Result<List<MyInvitationDto>>.Success(invitationDtos);
    }
}
