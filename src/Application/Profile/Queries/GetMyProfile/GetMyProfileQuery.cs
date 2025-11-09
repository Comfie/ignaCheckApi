using IgnaCheck.Application.Common.Interfaces;

namespace IgnaCheck.Application.Profile.Queries.GetMyProfile;

/// <summary>
/// Query to get the current user's profile.
/// </summary>
public record GetMyProfileQuery : IRequest<Result<UserProfileDto>>
{
}

/// <summary>
/// User profile DTO.
/// </summary>
public record UserProfileDto
{
    public string UserId { get; init; } = string.Empty;
    public string Email { get; init; } = string.Empty;
    public bool EmailConfirmed { get; init; }
    public string? FirstName { get; init; }
    public string? LastName { get; init; }
    public string FullName { get; init; } = string.Empty;
    public string? AvatarUrl { get; init; }
    public string? JobTitle { get; init; }
    public string? Department { get; init; }
    public string? PhoneNumber { get; init; }
    public bool PhoneNumberConfirmed { get; init; }
    public string? TimeZone { get; init; }
    public string? PreferredLanguage { get; init; }
    public string? NotificationPreferences { get; init; }
    public bool IsActive { get; init; }
    public DateTime? InvitedDate { get; init; }
    public DateTime? RegistrationCompletedDate { get; init; }
    public DateTime? LastLoginDate { get; init; }
    public DateTime CreatedDate { get; init; }
    public DateTime? LastModifiedDate { get; init; }
}

/// <summary>
/// Handler for the GetMyProfileQuery.
/// </summary>
public class GetMyProfileQueryHandler : IRequestHandler<GetMyProfileQuery, Result<UserProfileDto>>
{
    private readonly IUser _currentUser;
    private readonly IIdentityService _identityService;

    public GetMyProfileQueryHandler(
        IUser currentUser,
        IIdentityService identityService)
    {
        _currentUser = currentUser;
        _identityService = identityService;
    }

    public async Task<Result<UserProfileDto>> Handle(GetMyProfileQuery request, CancellationToken cancellationToken)
    {
        // Ensure user is authenticated
        if (string.IsNullOrEmpty(_currentUser.Id))
        {
            return Result<UserProfileDto>.Failure(new[] { "User must be authenticated." });
        }

        // Get user details
        var user = await _identityService.GetUserByIdAsync(_currentUser.Id);
        if (user is not Infrastructure.Identity.ApplicationUser appUser)
        {
            return Result<UserProfileDto>.Failure(new[] { "User not found." });
        }

        var profile = new UserProfileDto
        {
            UserId = appUser.Id,
            Email = appUser.Email ?? string.Empty,
            EmailConfirmed = appUser.EmailConfirmed,
            FirstName = appUser.FirstName,
            LastName = appUser.LastName,
            FullName = appUser.FullName,
            AvatarUrl = appUser.AvatarUrl,
            JobTitle = appUser.JobTitle,
            Department = appUser.Department,
            PhoneNumber = appUser.PhoneNumber,
            PhoneNumberConfirmed = appUser.PhoneNumberConfirmed,
            TimeZone = appUser.TimeZone,
            PreferredLanguage = appUser.PreferredLanguage,
            NotificationPreferences = appUser.NotificationPreferences,
            IsActive = appUser.IsActive,
            InvitedDate = appUser.InvitedDate,
            RegistrationCompletedDate = appUser.RegistrationCompletedDate,
            LastLoginDate = appUser.LastLoginDate,
            CreatedDate = appUser.Created,
            LastModifiedDate = appUser.LastModified
        };

        return Result<UserProfileDto>.Success(profile);
    }
}
