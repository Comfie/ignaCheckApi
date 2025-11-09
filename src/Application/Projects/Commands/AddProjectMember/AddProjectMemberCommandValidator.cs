namespace IgnaCheck.Application.Projects.Commands.AddProjectMember;

public class AddProjectMemberCommandValidator : AbstractValidator<AddProjectMemberCommand>
{
    public AddProjectMemberCommandValidator()
    {
        RuleFor(v => v.ProjectId)
            .NotEmpty().WithMessage("Project ID is required.");

        RuleFor(v => v.UserId)
            .NotEmpty().WithMessage("User ID is required.");

        RuleFor(v => v.Role)
            .IsInEnum().WithMessage("Valid role is required.");
    }
}
