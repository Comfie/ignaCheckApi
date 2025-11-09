namespace IgnaCheck.Application.Projects.Commands.ArchiveProject;

public class ArchiveProjectCommandValidator : AbstractValidator<ArchiveProjectCommand>
{
    public ArchiveProjectCommandValidator()
    {
        RuleFor(v => v.ProjectId)
            .NotEmpty().WithMessage("Project ID is required.");
    }
}
