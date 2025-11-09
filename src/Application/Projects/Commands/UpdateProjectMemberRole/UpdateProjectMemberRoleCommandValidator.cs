namespace IgnaCheck.Application.Projects.Commands.UpdateProjectMemberRole;

public class UpdateProjectMemberRoleCommandValidator : AbstractValidator<UpdateProjectMemberRoleCommand>
{
    public UpdateProjectMemberRoleCommandValidator()
    {
        RuleFor(v => v.ProjectId)
            .NotEmpty().WithMessage("Project ID is required.");

        RuleFor(v => v.UserId)
            .NotEmpty().WithMessage("User ID is required.");

        RuleFor(v => v.NewRole)
            .IsInEnum().WithMessage("Valid role is required.");
    }
}
