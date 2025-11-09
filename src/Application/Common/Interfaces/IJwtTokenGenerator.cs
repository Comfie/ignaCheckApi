namespace IgnaCheck.Application.Common.Interfaces;

/// <summary>
/// Service for generating JWT tokens.
/// </summary>
public interface IJwtTokenGenerator
{
    /// <summary>
    /// Generates a JWT access token for a user.
    /// </summary>
    /// <param name="userId">User's unique identifier</param>
    /// <param name="email">User's email address</param>
    /// <param name="firstName">User's first name</param>
    /// <param name="lastName">User's last name</param>
    /// <param name="roles">User's roles</param>
    /// <param name="organizationId">Current organization/workspace ID (optional)</param>
    /// <param name="organizationRole">User's role in the current organization (optional)</param>
    /// <param name="expiresInMinutes">Token expiration in minutes (default: 60)</param>
    /// <returns>JWT access token</returns>
    string GenerateAccessToken(
        string userId,
        string email,
        string? firstName,
        string? lastName,
        IEnumerable<string> roles,
        Guid? organizationId = null,
        string? organizationRole = null,
        int expiresInMinutes = 60);

    /// <summary>
    /// Generates a refresh token.
    /// </summary>
    /// <returns>Refresh token string</returns>
    string GenerateRefreshToken();

    /// <summary>
    /// Validates a JWT token and returns the user ID if valid.
    /// </summary>
    /// <param name="token">JWT token to validate</param>
    /// <returns>User ID if valid, null otherwise</returns>
    string? ValidateToken(string token);
}
