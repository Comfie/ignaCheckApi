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
    private readonly IApplicationDbContext _context;

    public UpdateMyProfileCommandHandler(
        IUser currentUser,
        IIdentityService identityService,
        IApplicationDbContext context)
    {
        _currentUser = currentUser;
        _identityService = identityService;
        _context = context;
    }

    public async Task<Result<UserProfileDto>> Handle(UpdateMyProfileCommand request, CancellationToken cancellationToken)
    {
        // Ensure user is authenticated
        if (string.IsNullOrEmpty(_currentUser.Id))
        {
            return Result<UserProfileDto>.Failure(new[] { "User must be authenticated." });
        }

        // Get user from database (need EF tracking)
        var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == _currentUser.Id, cancellationToken);
        if (user == null)
        {
            return Result<UserProfileDto>.Failure(new[] { "User not found." });
        }

        // Update fields if provided
        if (request.FirstName != null)
        {
            user.FirstName = request.FirstName;
        }

        if (request.LastName != null)
        {
            user.LastName = request.LastName;
        }

        if (request.JobTitle != null)
        {
            user.JobTitle = request.JobTitle;
        }

        if (request.Department != null)
        {
            user.Department = request.Department;
        }

        if (request.PhoneNumber != null)
        {
            user.PhoneNumber = request.PhoneNumber;
            // Reset phone number confirmation if changed
            if (user.PhoneNumber != request.PhoneNumber)
            {
                user.PhoneNumberConfirmed = false;
            }
        }

        if (request.TimeZone != null)
        {
            user.TimeZone = request.TimeZone;
        }

        if (request.PreferredLanguage != null)
        {
            user.PreferredLanguage = request.PreferredLanguage;
        }

        await _context.SaveChangesAsync(cancellationToken);

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
