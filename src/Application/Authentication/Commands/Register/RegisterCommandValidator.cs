namespace IgnaCheck.Application.Authentication.Commands.Register;

/// <summary>
/// Validator for the RegisterCommand.
/// </summary>
public class RegisterCommandValidator : AbstractValidator<RegisterCommand>
{
    public RegisterCommandValidator()
    {
        RuleFor(v => v.Email)
            .NotEmpty()
            .WithMessage("Email is required.")
            .EmailAddress()
            .WithMessage("Email must be a valid email address.")
            .MaximumLength(256)
            .WithMessage("Email must not exceed 256 characters.");

        RuleFor(v => v.Password)
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
            .Equal(v => v.Password)
            .WithMessage("Passwords do not match.");

        RuleFor(v => v.FirstName)
            .MaximumLength(100)
            .When(v => !string.IsNullOrEmpty(v.FirstName))
            .WithMessage("First name must not exceed 100 characters.");

        RuleFor(v => v.LastName)
            .MaximumLength(100)
            .When(v => !string.IsNullOrEmpty(v.LastName))
            .WithMessage("Last name must not exceed 100 characters.");

        RuleFor(v => v.WorkspaceName)
            .NotEmpty()
            .When(v => v.CreateWorkspace)
            .WithMessage("Workspace name is required when creating a workspace.")
            .MinimumLength(2)
            .When(v => v.CreateWorkspace && !string.IsNullOrWhiteSpace(v.WorkspaceName))
            .WithMessage("Workspace name must be at least 2 characters long.")
            .MaximumLength(200)
            .When(v => v.CreateWorkspace)
            .WithMessage("Workspace name must not exceed 200 characters.");
    }
}
