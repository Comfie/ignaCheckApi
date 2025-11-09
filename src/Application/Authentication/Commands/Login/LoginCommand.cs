using IgnaCheck.Application.Common.Interfaces;

namespace IgnaCheck.Application.Authentication.Commands.Login;

/// <summary>
/// Command to log in a user.
/// </summary>
public record LoginCommand : IRequest<Result<LoginResponse>>
{
    /// <summary>
    /// User's email address.
    /// </summary>
    public string Email { get; init; } = string.Empty;

    /// <summary>
    /// User's password.
    /// </summary>
    public string Password { get; init; } = string.Empty;

    /// <summary>
    /// Whether to remember the user (extend token expiration).
    /// </summary>
    public bool RememberMe { get; init; }
}

/// <summary>
/// Response for successful login.
/// </summary>
public record LoginResponse
{
    /// <summary>
    /// JWT access token.
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
    /// User ID.
    /// </summary>
    public string UserId { get; init; } = string.Empty;

    /// <summary>
    /// User's email address.
    /// </summary>
    public string Email { get; init; } = string.Empty;

    /// <summary>
    /// User's first name.
    /// </summary>
    public string? FirstName { get; init; }

    /// <summary>
    /// User's last name.
    /// </summary>
    public string? LastName { get; init; }
}

/// <summary>
/// Handler for the LoginCommand.
/// </summary>
public class LoginCommandHandler : IRequestHandler<LoginCommand, Result<LoginResponse>>
{
    private readonly IIdentityService _identityService;

    public LoginCommandHandler(IIdentityService identityService)
    {
        _identityService = identityService;
    }

    public async Task<Result<LoginResponse>> Handle(LoginCommand request, CancellationToken cancellationToken)
    {
        // Check credentials
        var userId = await _identityService.CheckPasswordAsync(request.Email, request.Password);

        if (userId == null)
        {
            return Result<LoginResponse>.Failure(new[] { "Invalid email or password." });
        }

        // Update last login date
        await _identityService.UpdateLastLoginDateAsync(userId);

        // Get user details
        var user = await _identityService.GetUserByIdAsync(userId);
        if (user is not IgnaCheck.Infrastructure.Identity.ApplicationUser appUser)
        {
            return Result<LoginResponse>.Failure(new[] { "User not found." });
        }

        // Check if email is verified
        if (!appUser.EmailConfirmed)
        {
            return Result<LoginResponse>.Failure(new[] { "Please verify your email address before logging in." });
        }

        // TODO: Generate JWT token (will implement in next step)
        // For now, return a placeholder response
        var expiresIn = request.RememberMe ? 2592000 : 3600; // 30 days or 1 hour

        var response = new LoginResponse
        {
            AccessToken = "TODO_GENERATE_JWT_TOKEN",
            TokenType = "Bearer",
            ExpiresIn = expiresIn,
            UserId = appUser.Id,
            Email = appUser.Email!,
            FirstName = appUser.FirstName,
            LastName = appUser.LastName
        };

        return Result<LoginResponse>.Success(response);
    }
}
