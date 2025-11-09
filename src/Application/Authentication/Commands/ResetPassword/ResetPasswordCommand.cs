using IgnaCheck.Application.Common.Interfaces;

namespace IgnaCheck.Application.Authentication.Commands.ResetPassword;

/// <summary>
/// Command to reset a user's password.
/// </summary>
public record ResetPasswordCommand : IRequest<Result>
{
    /// <summary>
    /// Password reset token.
    /// </summary>
    public string Token { get; init; } = string.Empty;

    /// <summary>
    /// User ID (extracted from token or provided).
    /// </summary>
    public string? UserId { get; init; }

    /// <summary>
    /// New password.
    /// </summary>
    public string NewPassword { get; init; } = string.Empty;

    /// <summary>
    /// Password confirmation (must match NewPassword).
    /// </summary>
    public string ConfirmPassword { get; init; } = string.Empty;
}

/// <summary>
/// Handler for the ResetPasswordCommand.
/// </summary>
public class ResetPasswordCommandHandler : IRequestHandler<ResetPasswordCommand, Result>
{
    private readonly IIdentityService _identityService;

    public ResetPasswordCommandHandler(IIdentityService identityService)
    {
        _identityService = identityService;
    }

    public async Task<Result> Handle(ResetPasswordCommand request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrEmpty(request.UserId))
        {
            return Result.Failure(new[] { "User ID is required." });
        }

        // Reset the password
        var result = await _identityService.ResetPasswordAsync(
            request.UserId,
            request.Token,
            request.NewPassword);

        return result;
    }
}
