using Microsoft.AspNetCore.Identity;

namespace IgnaCheck.Infrastructure.Identity;

/// <summary>
/// Represents an application user with extended profile and workspace information.
/// Inherits from IdentityUser to leverage ASP.NET Core Identity features.
/// </summary>
public class ApplicationUser : IdentityUser
{
    /// <summary>
    /// User's first name.
    /// </summary>
    public string? FirstName { get; set; }

    /// <summary>
    /// User's last name.
    /// </summary>
    public string? LastName { get; set; }

    /// <summary>
    /// User's full name (computed property for display purposes).
    /// </summary>
    public string FullName => $"{FirstName} {LastName}".Trim();

    /// <summary>
    /// URL or path to the user's avatar/profile picture.
    /// </summary>
    public string? AvatarUrl { get; set; }

    /// <summary>
    /// User's job title or role within their organization.
    /// </summary>
    public string? JobTitle { get; set; }

    /// <summary>
    /// User's department or team.
    /// </summary>
    public string? Department { get; set; }

    /// <summary>
    /// User's timezone (IANA timezone identifier, e.g., "America/New_York").
    /// </summary>
    public string? TimeZone { get; set; }

    /// <summary>
    /// User's preferred language (ISO 639-1 code, e.g., "en", "es").
    /// </summary>
    public string? PreferredLanguage { get; set; }

    /// <summary>
    /// Email notification preferences stored as JSON.
    /// Example: {"newFindings": true, "taskAssigned": true, "weeklyDigest": false}
    /// </summary>
    public string? NotificationPreferences { get; set; }

    /// <summary>
    /// Indicates whether the user's email has been verified.
    /// Duplicates IdentityUser.EmailConfirmed for convenience.
    /// </summary>
    public bool IsEmailVerified => EmailConfirmed;

    /// <summary>
    /// Indicates whether the user account is active.
    /// Inactive users cannot log in.
    /// </summary>
    public bool IsActive { get; set; } = true;

    /// <summary>
    /// Date when the user was invited to the platform.
    /// </summary>
    public DateTime? InvitedDate { get; set; }

    /// <summary>
    /// User who invited this user (by email).
    /// </summary>
    public string? InvitedBy { get; set; }

    /// <summary>
    /// Date when the user completed registration.
    /// </summary>
    public DateTime? RegistrationCompletedDate { get; set; }

    /// <summary>
    /// Date when the user last logged in.
    /// </summary>
    public DateTime? LastLoginDate { get; set; }

    /// <summary>
    /// Date when the user account was created.
    /// </summary>
    public DateTime CreatedDate { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Date when the user account was last updated.
    /// </summary>
    public DateTime? UpdatedDate { get; set; }

    /// <summary>
    /// User's bio or description (optional).
    /// </summary>
    public string? Bio { get; set; }

    /// <summary>
    /// External authentication provider (e.g., "Google", "Microsoft", "GitHub").
    /// Null for local authentication.
    /// </summary>
    public string? ExternalProvider { get; set; }

    /// <summary>
    /// External provider's user ID.
    /// </summary>
    public string? ExternalUserId { get; set; }
}
