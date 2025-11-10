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
        if (user == null)
        {
            return Result<UserProfileDto>.Failure(new[] { "User not found." });
        }

        // Return updated profile
        var profile = new UserProfileDto
        {
            UserId = user.Id,
            Email = user.Email ?? string.Empty,
            EmailConfirmed = user.EmailConfirmed,
            FirstName = user.FirstName,
            LastName = user.LastName,
            FullName = (user.FirstName + " " + user.LastName).Trim(),
            AvatarUrl = user.AvatarUrl,
            JobTitle = user.JobTitle,
            Department = user.Department,
            PhoneNumber = user.PhoneNumber,
            PhoneNumberConfirmed = user.PhoneNumberConfirmed,
            TimeZone = user.TimeZone,
            PreferredLanguage = user.PreferredLanguage,
            NotificationPreferences = user.NotificationPreferences,
            IsActive = user.IsActive,
            InvitedDate = user.InvitedDate,
            RegistrationCompletedDate = user.RegistrationCompletedDate,
            LastLoginDate = user.LastLoginDate,
            CreatedDate = user.Created,
            LastModifiedDate = user.LastModified
        };

        return Result<UserProfileDto>.Success(profile);
    }
}
