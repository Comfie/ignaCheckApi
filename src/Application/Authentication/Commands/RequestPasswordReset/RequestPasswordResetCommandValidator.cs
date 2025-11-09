namespace IgnaCheck.Application.Authentication.Commands.RequestPasswordReset;

/// <summary>
/// Validator for the RequestPasswordResetCommand.
/// </summary>
public class RequestPasswordResetCommandValidator : AbstractValidator<RequestPasswordResetCommand>
{
    public RequestPasswordResetCommandValidator()
    {
        RuleFor(v => v.Email)
            .NotEmpty()
            .WithMessage("Email is required.")
            .EmailAddress()
            .WithMessage("Email must be a valid email address.");
    }
}
