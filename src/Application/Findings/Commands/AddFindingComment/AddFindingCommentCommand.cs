using IgnaCheck.Application.Common.Interfaces;
using IgnaCheck.Domain.Entities;

namespace IgnaCheck.Application.Findings.Commands.AddFindingComment;

/// <summary>
/// Command to add a comment to a finding.
/// </summary>
public record AddFindingCommentCommand : IRequest<Result<Guid>>
{
    /// <summary>
    /// Finding ID.
    /// </summary>
    public Guid FindingId { get; init; }

    /// <summary>
    /// Parent comment ID for threaded replies (optional).
    /// </summary>
    public Guid? ParentCommentId { get; init; }

    /// <summary>
    /// Comment content (supports markdown).
    /// </summary>
    public string Content { get; init; } = string.Empty;

    /// <summary>
    /// User IDs mentioned in the comment (optional).
    /// </summary>
    public List<string>? Mentions { get; init; }

    /// <summary>
    /// Indicates if this comment marks the finding as resolved.
    /// </summary>
    public bool IsResolutionComment { get; init; }
}

/// <summary>
/// Validator for AddFindingCommentCommand.
/// </summary>
public class AddFindingCommentCommandValidator : AbstractValidator<AddFindingCommentCommand>
{
    public AddFindingCommentCommandValidator()
    {
        RuleFor(v => v.FindingId)
            .NotEmpty().WithMessage("Finding ID is required.");

        RuleFor(v => v.Content)
            .NotEmpty().WithMessage("Comment content is required.")
            .MaximumLength(10000).WithMessage("Comment content must not exceed 10000 characters.");
    }
}

/// <summary>
/// Handler for AddFindingCommentCommand.
/// </summary>
public class AddFindingCommentCommandHandler : IRequestHandler<AddFindingCommentCommand, Result<Guid>>
{
    private readonly IApplicationDbContext _context;
    private readonly IUser _currentUser;

    public AddFindingCommentCommandHandler(
        IApplicationDbContext context,
        IUser currentUser)
    {
        _context = context;
        _currentUser = currentUser;
    }

    public async Task<Result<Guid>> Handle(AddFindingCommentCommand request, CancellationToken cancellationToken)
    {
        // Ensure user is authenticated
        if (string.IsNullOrEmpty(_currentUser.Id))
        {
            return Result<Guid>.Failure(new[] { "User must be authenticated." });
        }

        // Get finding with project members
        var finding = await _context.ComplianceFindings
            .Include(f => f.Project)
            .ThenInclude(p => p.ProjectMembers)
            .Include(f => f.Comments)
            .FirstOrDefaultAsync(f => f.Id == request.FindingId, cancellationToken);

        if (finding == null)
        {
            return Result<Guid>.Failure(new[] { "Finding not found." });
        }

        // Check if user is a member of the project
        var isMember = finding.Project.ProjectMembers.Any(pm => pm.UserId == _currentUser.Id && pm.IsActive);
        if (!isMember)
        {
            return Result<Guid>.Failure(new[] { "Access denied. You are not a member of this project." });
        }

        // If this is a reply, verify parent comment exists
        if (request.ParentCommentId.HasValue)
        {
            var parentComment = finding.Comments.FirstOrDefault(c => c.Id == request.ParentCommentId.Value);
            if (parentComment == null)
            {
                return Result<Guid>.Failure(new[] { "Parent comment not found." });
            }
        }

        // Verify mentioned users are project members
        if (request.Mentions != null && request.Mentions.Any())
        {
            var invalidMentions = request.Mentions
                .Where(userId => !finding.Project.ProjectMembers.Any(pm => pm.UserId == userId && pm.IsActive))
                .ToList();

            if (invalidMentions.Any())
            {
                return Result<Guid>.Failure(new[] { "One or more mentioned users are not members of this project." });
            }
        }

        // Create comment
        var comment = new FindingComment
        {
            FindingId = request.FindingId,
            ParentCommentId = request.ParentCommentId,
            Content = request.Content,
            Mentions = request.Mentions != null && request.Mentions.Any()
                ? System.Text.Json.JsonSerializer.Serialize(request.Mentions)
                : null,
            IsResolutionComment = request.IsResolutionComment
        };

        _context.FindingComments.Add(comment);
        await _context.SaveChangesAsync(cancellationToken);

        return Result<Guid>.Success(comment.Id);
    }
}
