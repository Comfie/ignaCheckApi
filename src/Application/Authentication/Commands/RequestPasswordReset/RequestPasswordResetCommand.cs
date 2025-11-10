using IgnaCheck.Application.Common.Interfaces;

namespace IgnaCheck.Application.Authentication.Commands.RequestPasswordReset;

/// <summary>
/// Command to request a password reset.
/// </summary>
public record RequestPasswordResetCommand : IRequest<Result>
{
    /// <summary>
    /// User's email address.
    /// </summary>
    public string Email { get; init; } = string.Empty;
}

/// <summary>
/// Handler for the RequestPasswordResetCommand.
/// </summary>
public class RequestPasswordResetCommandHandler : IRequestHandler<RequestPasswordResetCommand, Result>
{
    private readonly IIdentityService _identityService;
    private readonly IEmailService _emailService;

    public RequestPasswordResetCommandHandler(
        IIdentityService identityService,
        IEmailService emailService)
    {
        _identityService = identityService;
        _emailService = emailService;
    }

    public async Task<Result> Handle(RequestPasswordResetCommand request, CancellationToken cancellationToken)
    {
        // Always return success to prevent email enumeration attacks
        // But only send email if user exists

        var user = await _identityService.GetUserByEmailAsync(request.Email);
        if (user != null)
        {
            // Generate password reset token
            var resetToken = await _identityService.GeneratePasswordResetTokenAsync(user.Id);

            // Send password reset email
            await _emailService.SendPasswordResetAsync(
                user.Email!,
                user.FirstName,
                resetToken,
                cancellationToken);
        }

        // Always return success (don't reveal if email exists)
        return Result.Success();
    }
}
