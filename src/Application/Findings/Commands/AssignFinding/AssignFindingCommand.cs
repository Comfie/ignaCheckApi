using IgnaCheck.Application.Common.Interfaces;
using IgnaCheck.Domain.Entities;

namespace IgnaCheck.Application.Findings.Commands.AssignFinding;

/// <summary>
/// Command to assign a finding to a team member.
/// </summary>
public record AssignFindingCommand : IRequest<Result>
{
    /// <summary>
    /// Finding ID.
    /// </summary>
    public Guid FindingId { get; init; }

    /// <summary>
    /// User ID to assign to (null to unassign).
    /// </summary>
    public string? AssignedTo { get; init; }

    /// <summary>
    /// Due date for remediation (optional).
    /// </summary>
    public DateTime? DueDate { get; init; }
}

/// <summary>
/// Validator for AssignFindingCommand.
/// </summary>
public class AssignFindingCommandValidator : AbstractValidator<AssignFindingCommand>
{
    public AssignFindingCommandValidator()
    {
        RuleFor(v => v.FindingId)
            .NotEmpty().WithMessage("Finding ID is required.");

        RuleFor(v => v.DueDate)
            .GreaterThan(DateTime.UtcNow)
            .When(v => v.DueDate.HasValue)
            .WithMessage("Due date must be in the future.");
    }
}

/// <summary>
/// Handler for AssignFindingCommand.
/// </summary>
public class AssignFindingCommandHandler : IRequestHandler<AssignFindingCommand, Result>
{
    private readonly IApplicationDbContext _context;
    private readonly IUser _currentUser;

    public AssignFindingCommandHandler(
        IApplicationDbContext context,
        IUser currentUser)
    {
        _context = context;
        _currentUser = currentUser;
    }

    public async Task<Result> Handle(AssignFindingCommand request, CancellationToken cancellationToken)
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

        // Only owners and admins can assign findings
        if (member.Role == Domain.Enums.ProjectRole.Viewer || member.Role == Domain.Enums.ProjectRole.Contributor)
        {
            return Result.Failure(new[] { "Access denied. Only project owners can assign findings." });
        }

        // If assigning to someone, verify they are a member of the project
        if (!string.IsNullOrWhiteSpace(request.AssignedTo))
        {
            var assignee = finding.Project.ProjectMembers.FirstOrDefault(pm => pm.UserId == request.AssignedTo && pm.IsActive);
            if (assignee == null)
            {
                return Result.Failure(new[] { "The specified user is not a member of this project." });
            }
        }

        // Update assignment
        finding.AssignedTo = request.AssignedTo;

        // Update due date if provided
        if (request.DueDate.HasValue)
        {
            finding.DueDate = request.DueDate.Value;
        }

        await _context.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
