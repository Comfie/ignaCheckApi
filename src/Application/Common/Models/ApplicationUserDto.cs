namespace IgnaCheck.Application.Common.Models;

/// <summary>
/// Data transfer object representing an application user.
/// Used to abstract Infrastructure identity implementation from the Application layer.
/// </summary>
public class ApplicationUserDto
{
    /// <summary>
    /// User's unique identifier.
    /// </summary>
    public required string Id { get; init; }

    /// <summary>
    /// User's email address.
    /// </summary>
    public string? Email { get; init; }

    /// <summary>
    /// User's username.
    /// </summary>
    public string? UserName { get; init; }

    /// <summary>
    /// User's first name.
    /// </summary>
    public string? FirstName { get; init; }

    /// <summary>
    /// User's last name.
    /// </summary>
    public string? LastName { get; init; }

    /// <summary>
    /// User's full name (computed property).
    /// </summary>
    public string FullName => $"{FirstName} {LastName}".Trim();

    /// <summary>
    /// URL or path to the user's avatar/profile picture.
    /// </summary>
    public string? AvatarUrl { get; init; }

    /// <summary>
    /// User's job title or role within their organization.
    /// </summary>
    public string? JobTitle { get; init; }

    /// <summary>
    /// User's department or team.
    /// </summary>
    public string? Department { get; init; }

    /// <summary>
    /// User's phone number.
    /// </summary>
    public string? PhoneNumber { get; init; }

    /// <summary>
    /// Indicates whether the user's phone number has been confirmed.
    /// </summary>
    public bool PhoneNumberConfirmed { get; init; }

    /// <summary>
    /// User's timezone (IANA timezone identifier).
    /// </summary>
    public string? TimeZone { get; init; }

    /// <summary>
    /// User's preferred language (ISO 639-1 code).
    /// </summary>
    public string? PreferredLanguage { get; init; }

    /// <summary>
    /// Email notification preferences stored as JSON.
    /// </summary>
    public string? NotificationPreferences { get; init; }

    /// <summary>
    /// Indicates whether the user's email has been confirmed.
    /// </summary>
    public bool EmailConfirmed { get; init; }

    /// <summary>
    /// Indicates whether the user account is active.
    /// </summary>
    public bool IsActive { get; init; }

    /// <summary>
    /// Date when the user was invited to the platform.
    /// </summary>
    public DateTime? InvitedDate { get; init; }

    /// <summary>
    /// Date when the user completed registration.
    /// </summary>
    public DateTime? RegistrationCompletedDate { get; init; }

    /// <summary>
    /// Date when the user last logged in.
    /// </summary>
    public DateTime? LastLoginDate { get; init; }

    /// <summary>
    /// Date when the user account was created.
    /// </summary>
    public DateTime Created { get; init; }

    /// <summary>
    /// Date when the user account was last modified.
    /// </summary>
    public DateTime? LastModified { get; init; }

    /// <summary>
    /// User's bio or description.
    /// </summary>
    public string? Bio { get; init; }
}
