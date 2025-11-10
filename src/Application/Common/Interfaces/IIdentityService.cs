using IgnaCheck.Application.Common.Models;

namespace IgnaCheck.Application.Common.Interfaces;

/// <summary>
/// Service for managing user identity and authentication.
/// </summary>
public interface IIdentityService
{
    // Legacy methods (for backward compatibility)
    Task<string?> GetUserNameAsync(string userId);
    Task<bool> IsInRoleAsync(string userId, string role);
    Task<bool> AuthorizeAsync(string userId, string policyName);
    Task<(Result Result, string UserId)> CreateUserAsync(string userName, string password);
    Task<Result> DeleteUserAsync(string userId);

    // Enhanced user management methods

    /// <summary>
    /// Creates a new user with extended profile information.
    /// </summary>
    Task<Result<string>> CreateUserAsync(
        string email,
        string password,
        string? firstName,
        string? lastName);

    /// <summary>
    /// Gets a user by email address.
    /// </summary>
    Task<ApplicationUserDto?> GetUserByEmailAsync(string email);

    /// <summary>
    /// Gets a user by ID.
    /// </summary>
    Task<ApplicationUserDto?> GetUserByIdAsync(string userId);

    // Email verification methods

    /// <summary>
    /// Generates an email verification token for a user.
    /// </summary>
    Task<string> GenerateEmailVerificationTokenAsync(string userId);

    /// <summary>
    /// Verifies a user's email address using the provided token.
    /// </summary>
    Task<Result> VerifyEmailAsync(string userId, string token);

    // Password reset methods

    /// <summary>
    /// Generates a password reset token for a user.
    /// </summary>
    Task<string> GeneratePasswordResetTokenAsync(string userId);

    /// <summary>
    /// Resets a user's password using the provided token.
    /// </summary>
    Task<Result> ResetPasswordAsync(string userId, string token, string newPassword);

    // Authentication methods

    /// <summary>
    /// Checks if the provided credentials are valid.
    /// Returns the user ID if successful, null otherwise.
    /// </summary>
    Task<string?> CheckPasswordAsync(string email, string password);

    /// <summary>
    /// Updates the user's last login date.
    /// </summary>
    Task UpdateLastLoginDateAsync(string userId);

    // Profile management methods

    /// <summary>
    /// Updates the user's avatar URL.
    /// </summary>
    Task<bool> UpdateUserAvatarAsync(string userId, string avatarUrl);

    /// <summary>
    /// Updates the user's profile information.
    /// </summary>
    Task<bool> UpdateUserProfileAsync(
        string userId,
        string? firstName,
        string? lastName,
        string? jobTitle,
        string? department,
        string? phoneNumber,
        string? timeZone,
        string? preferredLanguage);

    /// <summary>
    /// Updates the user's notification preferences.
    /// </summary>
    Task<bool> UpdateNotificationPreferencesAsync(string userId, string notificationPreferences);

    /// <summary>
    /// Gets the user's notification preferences.
    /// </summary>
    Task<string?> GetNotificationPreferencesAsync(string userId);
}

