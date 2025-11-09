namespace IgnaCheck.Application.Documents.Commands.DeleteDocument;

public class DeleteDocumentCommandValidator : AbstractValidator<DeleteDocumentCommand>
{
    public DeleteDocumentCommandValidator()
    {
        RuleFor(v => v.DocumentId)
            .NotEmpty().WithMessage("Document ID is required.");
    }
}
