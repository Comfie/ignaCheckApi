using IgnaCheck.Application.Common.Interfaces;
using IgnaCheck.Application.Profile.Queries.GetMyProfile;

namespace IgnaCheck.Application.Profile.Commands.UpdateMyProfile;

/// <summary>
/// Command to update the current user's profile.
/// </summary>
public record UpdateMyProfileCommand : IRequest<Result<UserProfileDto>>
{
    /// <summary>
    /// First name.
    /// </summary>
    public string? FirstName { get; init; }

    /// <summary>
    /// Last name.
    /// </summary>
    public string? LastName { get; init; }

    /// <summary>
    /// Job title.
    /// </summary>
    public string? JobTitle { get; init; }

    /// <summary>
    /// Department.
    /// </summary>
    public string? Department { get; init; }

    /// <summary>
    /// Phone number.
    /// </summary>
    public string? PhoneNumber { get; init; }

    /// <summary>
    /// Time zone (IANA timezone identifier, e.g., "America/New_York").
    /// </summary>
    public string? TimeZone { get; init; }

    /// <summary>
    /// Preferred language (ISO 639-1 code, e.g., "en", "es").
    /// </summary>
    public string? PreferredLanguage { get; init; }
}

/// <summary>
/// Handler for the UpdateMyProfileCommand.
/// </summary>
public class UpdateMyProfileCommandHandler : IRequestHandler<UpdateMyProfileCommand, Result<UserProfileDto>>
{
    private readonly IUser _currentUser;
    private readonly IIdentityService _identityService;

    public UpdateMyProfileCommandHandler(
        IUser currentUser,
        IIdentityService identityService)
    {
        _currentUser = currentUser;
        _identityService = identityService;
    }

    public async Task<Result<UserProfileDto>> Handle(UpdateMyProfileCommand request, CancellationToken cancellationToken)
    {
        // Ensure user is authenticated
        if (string.IsNullOrEmpty(_currentUser.Id))
        {
            return Result<UserProfileDto>.Failure(new[] { "User must be authenticated." });
        }

        // Update profile using identity service
        var updateResult = await _identityService.UpdateUserProfileAsync(
            _currentUser.Id,
            request.FirstName,
            request.LastName,
            request.JobTitle,
            request.Department,
            request.PhoneNumber,
            request.TimeZone,
            request.PreferredLanguage
        );

        if (!updateResult)
        {
            return Result<UserProfileDto>.Failure(new[] { "Failed to update profile." });
        }

        // Get updated user details
        var user = await _identityService.GetUserByIdAsync(_currentUser.Id);
        if (user is not Infrastructure.Identity.ApplicationUser appUser)
        {
            return Result<UserProfileDto>.Failure(new[] { "User not found." });
        }

        // Return updated profile
        var profile = new UserProfileDto
        {
            UserId = appUser.Id,
            Email = appUser.Email ?? string.Empty,
            EmailConfirmed = appUser.EmailConfirmed,
            FirstName = appUser.FirstName,
            LastName = appUser.LastName,
            FullName = (appUser.FirstName + " " + appUser.LastName).Trim(),
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
