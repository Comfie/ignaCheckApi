namespace IgnaCheck.Application.Projects.Commands.RemoveProjectMember;

public class RemoveProjectMemberCommandValidator : AbstractValidator<RemoveProjectMemberCommand>
{
    public RemoveProjectMemberCommandValidator()
    {
        RuleFor(v => v.ProjectId)
            .NotEmpty().WithMessage("Project ID is required.");

        RuleFor(v => v.UserId)
            .NotEmpty().WithMessage("User ID is required.");
    }
}
