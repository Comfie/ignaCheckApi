namespace IgnaCheck.Application.Workspaces.Commands.CreateWorkspace;

/// <summary>
/// Validator for CreateWorkspaceCommand.
/// </summary>
public class CreateWorkspaceCommandValidator : AbstractValidator<CreateWorkspaceCommand>
{
    public CreateWorkspaceCommandValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Workspace name is required.")
            .MaximumLength(100).WithMessage("Workspace name must not exceed 100 characters.")
            .MinimumLength(2).WithMessage("Workspace name must be at least 2 characters.");

        RuleFor(x => x.Description)
            .MaximumLength(500).WithMessage("Description must not exceed 500 characters.")
            .When(x => !string.IsNullOrEmpty(x.Description));

        RuleFor(x => x.Slug)
            .MaximumLength(100).WithMessage("Slug must not exceed 100 characters.")
            .Matches(@"^[a-z0-9-]+$").WithMessage("Slug must contain only lowercase letters, numbers, and hyphens.")
            .When(x => !string.IsNullOrEmpty(x.Slug));

        RuleFor(x => x.SubscriptionTier)
            .Must(tier => tier == null || new[] { "Free", "Pro", "Enterprise" }.Contains(tier))
            .WithMessage("Subscription tier must be one of: Free, Pro, Enterprise.");

        RuleFor(x => x.BillingEmail)
            .EmailAddress().WithMessage("Billing email must be a valid email address.")
            .When(x => !string.IsNullOrEmpty(x.BillingEmail));

        RuleFor(x => x.CompanyName)
            .MaximumLength(200).WithMessage("Company name must not exceed 200 characters.")
            .When(x => !string.IsNullOrEmpty(x.CompanyName));
    }
}
