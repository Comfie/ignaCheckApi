namespace IgnaCheck.Application.Documents.Commands.UploadDocument;

public class UploadDocumentCommandValidator : AbstractValidator<UploadDocumentCommand>
{
    public UploadDocumentCommandValidator()
    {
        RuleFor(v => v.ProjectId)
            .NotEmpty().WithMessage("Project ID is required.");

        RuleFor(v => v.File)
            .NotNull().WithMessage("File is required.");

        RuleFor(v => v.Description)
            .MaximumLength(1000).WithMessage("Description must not exceed 1000 characters.")
            .When(v => !string.IsNullOrEmpty(v.Description));

        RuleFor(v => v.Tags)
            .MaximumLength(500).WithMessage("Tags must not exceed 500 characters.")
            .When(v => !string.IsNullOrEmpty(v.Tags));
    }
}
