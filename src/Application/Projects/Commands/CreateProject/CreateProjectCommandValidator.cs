namespace IgnaCheck.Application.Projects.Commands.CreateProject;

public class CreateProjectCommandValidator : AbstractValidator<CreateProjectCommand>
{
    public CreateProjectCommandValidator()
    {
        RuleFor(v => v.Name)
            .NotEmpty().WithMessage("Project name is required.")
            .MinimumLength(3).WithMessage("Project name must be at least 3 characters.")
            .MaximumLength(100).WithMessage("Project name must not exceed 100 characters.");

        RuleFor(v => v.Description)
            .MaximumLength(500).WithMessage("Project description must not exceed 500 characters.")
            .When(v => !string.IsNullOrEmpty(v.Description));

        RuleFor(v => v.TargetDate)
            .GreaterThan(DateTime.UtcNow).WithMessage("Target date must be in the future.")
            .When(v => v.TargetDate.HasValue);
    }
}
