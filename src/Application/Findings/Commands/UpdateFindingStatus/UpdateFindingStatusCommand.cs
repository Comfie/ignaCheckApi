using IgnaCheck.Application.Common.Interfaces;
using IgnaCheck.Domain.Enums;

namespace IgnaCheck.Application.Findings.Commands.UpdateFindingStatus;

/// <summary>
/// Command to update a finding's workflow status.
/// </summary>
public record UpdateFindingStatusCommand : IRequest<Result>
{
    /// <summary>
    /// Finding ID.
    /// </summary>
    public Guid FindingId { get; init; }

    /// <summary>
    /// New workflow status.
    /// </summary>
    public FindingWorkflowStatus WorkflowStatus { get; init; }

    /// <summary>
    /// Resolution notes (required when marking as Resolved).
    /// </summary>
    public string? ResolutionNotes { get; init; }
}

/// <summary>
/// Validator for UpdateFindingStatusCommand.
/// </summary>
public class UpdateFindingStatusCommandValidator : AbstractValidator<UpdateFindingStatusCommand>
{
    public UpdateFindingStatusCommandValidator()
    {
        RuleFor(v => v.FindingId)
            .NotEmpty().WithMessage("Finding ID is required.");

        RuleFor(v => v.WorkflowStatus)
            .IsInEnum().WithMessage("Invalid workflow status.");

        RuleFor(v => v.ResolutionNotes)
            .NotEmpty()
            .When(v => v.WorkflowStatus == FindingWorkflowStatus.Resolved)
            .WithMessage("Resolution notes are required when marking a finding as resolved.");

        RuleFor(v => v.ResolutionNotes)
            .MaximumLength(5000)
            .When(v => !string.IsNullOrWhiteSpace(v.ResolutionNotes))
            .WithMessage("Resolution notes must not exceed 5000 characters.");
    }
}

/// <summary>
/// Handler for UpdateFindingStatusCommand.
/// </summary>
public class UpdateFindingStatusCommandHandler : IRequestHandler<UpdateFindingStatusCommand, Result>
{
    private readonly IApplicationDbContext _context;
    private readonly IUser _currentUser;

    public UpdateFindingStatusCommandHandler(
        IApplicationDbContext context,
        IUser currentUser)
    {
        _context = context;
        _currentUser = currentUser;
    }

    public async Task<Result> Handle(UpdateFindingStatusCommand request, CancellationToken cancellationToken)
    {
        // Ensure user is authenticated
        if (string.IsNullOrEmpty(_currentUser.Id))
        {
            return Result.Failure(new[] { "User must be authenticated." });
        }

        // Get finding with project members
        var finding = await _context.ComplianceFindings
            .Include(f => f.Project)
            .ThenInclude(p => p.ProjectMembers)
            .FirstOrDefaultAsync(f => f.Id == request.FindingId, cancellationToken);

        if (finding == null)
        {
            return Result.Failure(new[] { "Finding not found." });
        }

        // Check if user is a member of the project
        var member = finding.Project.ProjectMembers.FirstOrDefault(pm => pm.UserId == _currentUser.Id && pm.IsActive);
        if (member == null)
        {
            return Result.Failure(new[] { "Access denied. You are not a member of this project." });
        }

        // Only owners and contributors can update status
        if (member.Role == ProjectRole.Viewer)
        {
            return Result.Failure(new[] { "Access denied. Viewers cannot update finding status." });
        }

        // Update workflow status
        finding.WorkflowStatus = request.WorkflowStatus;

        // If marking as resolved, update resolved fields
        if (request.WorkflowStatus == FindingWorkflowStatus.Resolved)
        {
            finding.ResolvedDate = DateTime.UtcNow;
            finding.ResolvedBy = _currentUser.Id;
            finding.ResolutionNotes = request.ResolutionNotes;
        }
        // If changing from resolved to another status, clear resolved fields
        else if (finding.ResolvedDate.HasValue)
        {
            finding.ResolvedDate = null;
            finding.ResolvedBy = null;
            // Keep resolution notes for audit trail
        }

        await _context.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
