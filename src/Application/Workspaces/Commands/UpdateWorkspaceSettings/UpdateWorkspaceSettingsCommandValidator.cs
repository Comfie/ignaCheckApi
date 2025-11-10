namespace IgnaCheck.Application.Workspaces.Commands.UpdateWorkspaceSettings;

/// <summary>
/// Validator for UpdateWorkspaceSettingsCommand.
/// </summary>
public class UpdateWorkspaceSettingsCommandValidator : AbstractValidator<UpdateWorkspaceSettingsCommand>
{
    public UpdateWorkspaceSettingsCommandValidator()
    {
        RuleFor(x => x.Name)
            .MaximumLength(100).WithMessage("Workspace name must not exceed 100 characters.")
            .MinimumLength(2).WithMessage("Workspace name must be at least 2 characters.")
            .When(x => !string.IsNullOrEmpty(x.Name));

        RuleFor(x => x.Description)
            .MaximumLength(500).WithMessage("Description must not exceed 500 characters.")
            .When(x => x.Description != null);

        RuleFor(x => x.Slug)
            .MaximumLength(100).WithMessage("Slug must not exceed 100 characters.")
            .Matches(@"^[a-z0-9-]+$").WithMessage("Slug must contain only lowercase letters, numbers, and hyphens.")
            .When(x => !string.IsNullOrEmpty(x.Slug));

        RuleFor(x => x.BillingEmail)
            .EmailAddress().WithMessage("Billing email must be a valid email address.")
            .When(x => !string.IsNullOrEmpty(x.BillingEmail));

        // TODO: The following properties need to be added to Organization.Settings JSON field:
        // - CompanyName, TaxId, BillingAddress, DefaultCurrency, DefaultLanguage

        // RuleFor(x => x.CompanyName)
        //     .MaximumLength(200).WithMessage("Company name must not exceed 200 characters.")
        //     .When(x => x.CompanyName != null);

        // RuleFor(x => x.TaxId)
        //     .MaximumLength(50).WithMessage("Tax ID must not exceed 50 characters.")
        //     .When(x => x.TaxId != null);

        // RuleFor(x => x.BillingAddress)
        //     .MaximumLength(500).WithMessage("Billing address must not exceed 500 characters.")
        //     .When(x => x.BillingAddress != null);

        // RuleFor(x => x.DefaultCurrency)
        //     .MaximumLength(3).WithMessage("Currency code must not exceed 3 characters.")
        //     .Matches(@"^[A-Z]{3}$").WithMessage("Currency code must be a 3-letter ISO currency code (e.g., USD, EUR).")
        //     .When(x => !string.IsNullOrEmpty(x.DefaultCurrency));

        // RuleFor(x => x.DefaultLanguage)
        //     .MaximumLength(10).WithMessage("Language code must not exceed 10 characters.")
        //     .When(x => x.DefaultLanguage != null);
    }
}
