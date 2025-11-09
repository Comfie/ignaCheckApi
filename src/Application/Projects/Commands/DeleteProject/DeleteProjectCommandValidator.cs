namespace IgnaCheck.Application.Projects.Commands.DeleteProject;

public class DeleteProjectCommandValidator : AbstractValidator<DeleteProjectCommand>
{
    public DeleteProjectCommandValidator()
    {
        RuleFor(v => v.ProjectId)
            .NotEmpty().WithMessage("Project ID is required.");

        RuleFor(v => v.ConfirmationName)
            .NotEmpty().WithMessage("Confirmation name is required for project deletion.");
    }
}
