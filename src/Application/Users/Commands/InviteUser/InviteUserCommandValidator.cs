namespace IgnaCheck.Application.Users.Commands.InviteUser;

/// <summary>
/// Validator for InviteUserCommand.
/// </summary>
public class InviteUserCommandValidator : AbstractValidator<InviteUserCommand>
{
    public InviteUserCommandValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email address is required.")
            .EmailAddress().WithMessage("Email address must be valid.")
            .MaximumLength(256).WithMessage("Email address must not exceed 256 characters.");

        RuleFor(x => x.Role)
            .NotEmpty().WithMessage("Role is required.")
            .Must(role => new[] {
                Domain.Constants.WorkspaceRoles.Owner,
                Domain.Constants.WorkspaceRoles.Admin,
                Domain.Constants.WorkspaceRoles.Member,
                Domain.Constants.WorkspaceRoles.Viewer
            }.Contains(role))
            .WithMessage("Role must be one of: Owner, Admin, Member, Viewer.");

        RuleFor(x => x.Message)
            .MaximumLength(500).WithMessage("Message must not exceed 500 characters.")
            .When(x => !string.IsNullOrEmpty(x.Message));
    }
}
