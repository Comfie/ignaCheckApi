namespace IgnaCheck.Application.Users.Commands.UpdateMemberRole;

/// <summary>
/// Validator for UpdateMemberRoleCommand.
/// </summary>
public class UpdateMemberRoleCommandValidator : AbstractValidator<UpdateMemberRoleCommand>
{
    public UpdateMemberRoleCommandValidator()
    {
        RuleFor(x => x.UserId)
            .NotEmpty().WithMessage("User ID is required.");

        RuleFor(x => x.NewRole)
            .NotEmpty().WithMessage("New role is required.")
            .Must(role => new[] {
                Domain.Constants.WorkspaceRoles.Owner,
                Domain.Constants.WorkspaceRoles.Admin,
                Domain.Constants.WorkspaceRoles.Member,
                Domain.Constants.WorkspaceRoles.Viewer
            }.Contains(role))
            .WithMessage("Role must be one of: Owner, Admin, Member, Viewer.");
    }
}
