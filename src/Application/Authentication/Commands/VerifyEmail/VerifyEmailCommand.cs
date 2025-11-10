using IgnaCheck.Application.Common.Interfaces;

namespace IgnaCheck.Application.Authentication.Commands.VerifyEmail;

/// <summary>
/// Command to verify a user's email address.
/// </summary>
public record VerifyEmailCommand : IRequest<Result>
{
    /// <summary>
    /// Email verification token.
    /// </summary>
    public string Token { get; init; } = string.Empty;

    /// <summary>
    /// User ID (extracted from token or provided).
    /// </summary>
    public string? UserId { get; init; }
}

/// <summary>
/// Handler for the VerifyEmailCommand.
/// </summary>
public class VerifyEmailCommandHandler : IRequestHandler<VerifyEmailCommand, Result>
{
    private readonly IIdentityService _identityService;
    private readonly IEmailService _emailService;

    public VerifyEmailCommandHandler(
        IIdentityService identityService,
        IEmailService emailService)
    {
        _identityService = identityService;
        _emailService = emailService;
    }

    public async Task<Result> Handle(VerifyEmailCommand request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrEmpty(request.UserId))
        {
            return Result.Failure(new[] { "User ID is required." });
        }

        // Verify the email
        var result = await _identityService.VerifyEmailAsync(request.UserId, request.Token);

        if (!result.Succeeded)
        {
            return result;
        }

        // Send welcome email
        var user = await _identityService.GetUserByIdAsync(request.UserId);
        if (user != null)
        {
            await _emailService.SendWelcomeEmailAsync(
                user.Email!,
                user.FirstName,
                cancellationToken);
        }

        return Result.Success();
    }
}
