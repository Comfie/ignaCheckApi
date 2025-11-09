namespace IgnaCheck.Application.Authentication.Commands.ResetPassword;

/// <summary>
/// Validator for the ResetPasswordCommand.
/// </summary>
public class ResetPasswordCommandValidator : AbstractValidator<ResetPasswordCommand>
{
    public ResetPasswordCommandValidator()
    {
        RuleFor(v => v.Token)
            .NotEmpty()
            .WithMessage("Reset token is required.");

        RuleFor(v => v.UserId)
            .NotEmpty()
            .WithMessage("User ID is required.");

        RuleFor(v => v.NewPassword)
            .NotEmpty()
            .WithMessage("Password is required.")
            .MinimumLength(8)
            .WithMessage("Password must be at least 8 characters long.")
            .Matches(@"[A-Z]")
            .WithMessage("Password must contain at least one uppercase letter.")
            .Matches(@"[a-z]")
            .WithMessage("Password must contain at least one lowercase letter.")
            .Matches(@"[0-9]")
            .WithMessage("Password must contain at least one number.")
            .Matches(@"[\W_]")
            .WithMessage("Password must contain at least one special character.");

        RuleFor(v => v.ConfirmPassword)
            .NotEmpty()
            .WithMessage("Password confirmation is required.")
            .Equal(v => v.NewPassword)
            .WithMessage("Passwords do not match.");
    }
}
