using IgnaCheck.Application.Common.Interfaces;
using IgnaCheck.Domain.Entities;
using IgnaCheck.Domain.Enums;

namespace IgnaCheck.Application.Findings.Queries.GetFindingDetails;

/// <summary>
/// Query to get detailed information about a specific finding.
/// </summary>
public record GetFindingDetailsQuery : IRequest<Result<FindingDetailsDto>>
{
    /// <summary>
    /// Finding ID.
    /// </summary>
    public Guid FindingId { get; init; }
}

/// <summary>
/// Detailed finding information DTO.
/// </summary>
public record FindingDetailsDto
{
    public Guid Id { get; init; }
    public string FindingCode { get; init; } = string.Empty;
    public string Title { get; init; } = string.Empty;
    public string Description { get; init; } = string.Empty;
    public ComplianceStatus Status { get; init; }
    public FindingWorkflowStatus WorkflowStatus { get; init; }
    public RiskLevel RiskLevel { get; init; }
    public string? RemediationGuidance { get; init; }
    public decimal? EstimatedEffort { get; init; }
    public decimal? ConfidenceScore { get; init; }
    public bool IsReviewed { get; init; }
    public string? ReviewedBy { get; init; }
    public DateTime? ReviewedDate { get; init; }
    public string? ReviewNotes { get; init; }
    public DateTime? DueDate { get; init; }
    public string? AssignedTo { get; init; }
    public DateTime? ResolvedDate { get; init; }
    public string? ResolvedBy { get; init; }
    public string? ResolutionNotes { get; init; }
    public DateTime? LastAnalysisDate { get; init; }
    public int AnalysisVersion { get; init; }
    public string? AnalysisModel { get; init; }
    public DateTime CreatedAt { get; init; }
    public DateTime? LastModified { get; init; }

    // Control information
    public ControlDto Control { get; init; } = null!;

    // Framework information
    public FrameworkDto Framework { get; init; } = null!;

    // Evidence
    public List<EvidenceDto> Evidence { get; init; } = new();

    // Comments
    public List<CommentDto> Comments { get; init; } = new();

    // Remediation task
    public Guid? RemediationTaskId { get; init; }
}

public record ControlDto
{
    public Guid Id { get; init; }
    public string ControlReference { get; init; } = string.Empty;
    public string Title { get; init; } = string.Empty;
    public string Description { get; init; } = string.Empty;
    public string? ImplementationGuidance { get; init; }
}

public record FrameworkDto
{
    public Guid Id { get; init; }
    public string Code { get; init; } = string.Empty;
    public string Name { get; init; } = string.Empty;
    public string? Version { get; init; }
}

public record EvidenceDto
{
    public Guid Id { get; init; }
    public Guid DocumentId { get; init; }
    public string DocumentName { get; init; } = string.Empty;
    public EvidenceType EvidenceType { get; init; }
    public string? Excerpt { get; init; }
    public string? PageReference { get; init; }
    public decimal? RelevanceScore { get; init; }
    public string? Notes { get; init; }
    public bool IsManuallyAdded { get; init; }
}

public record CommentDto
{
    public Guid Id { get; init; }
    public Guid? ParentCommentId { get; init; }
    public string Content { get; init; } = string.Empty;
    public string CreatedBy { get; init; } = string.Empty;
    public DateTime CreatedAt { get; init; }
    public bool IsEdited { get; init; }
    public DateTime? EditedDate { get; init; }
    public bool IsResolutionComment { get; init; }
    public List<string> Mentions { get; init; } = new();
    public List<CommentDto> Replies { get; init; } = new();
}

/// <summary>
/// Handler for GetFindingDetailsQuery.
/// </summary>
public class GetFindingDetailsQueryHandler : IRequestHandler<GetFindingDetailsQuery, Result<FindingDetailsDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly IUser _currentUser;

    public GetFindingDetailsQueryHandler(
        IApplicationDbContext context,
        IUser currentUser)
    {
        _context = context;
        _currentUser = currentUser;
    }

    public async Task<Result<FindingDetailsDto>> Handle(GetFindingDetailsQuery request, CancellationToken cancellationToken)
    {
        // Ensure user is authenticated
        if (string.IsNullOrEmpty(_currentUser.Id))
        {
            return Result<FindingDetailsDto>.Failure(new[] { "User must be authenticated." });
        }

        // Get finding with all related data
        var finding = await _context.ComplianceFindings
            .Include(f => f.Control)
            .ThenInclude(c => c.Framework)
            .Include(f => f.Evidence)
            .ThenInclude(e => e.Document)
            .Include(f => f.Comments.Where(c => c.ParentCommentId == null))
            .ThenInclude(c => c.Replies)
            .Include(f => f.Project)
            .ThenInclude(p => p.ProjectMembers)
            .FirstOrDefaultAsync(f => f.Id == request.FindingId, cancellationToken);

        if (finding == null)
        {
            return Result<FindingDetailsDto>.Failure(new[] { "Finding not found." });
        }

        // Check if user is a member of the project
        var isMember = finding.Project.ProjectMembers.Any(pm => pm.UserId == _currentUser.Id && pm.IsActive);
        if (!isMember)
        {
            return Result<FindingDetailsDto>.Failure(new[] { "Access denied. You are not a member of this project." });
        }

        // Map to DTO
        var result = new FindingDetailsDto
        {
            Id = finding.Id,
            FindingCode = finding.FindingCode,
            Title = finding.Title,
            Description = finding.Description,
            Status = finding.Status,
            WorkflowStatus = finding.WorkflowStatus,
            RiskLevel = finding.RiskLevel,
            RemediationGuidance = finding.RemediationGuidance,
            EstimatedEffort = finding.EstimatedEffort,
            ConfidenceScore = finding.ConfidenceScore,
            IsReviewed = finding.IsReviewed,
            ReviewedBy = finding.ReviewedBy,
            ReviewedDate = finding.ReviewedDate,
            ReviewNotes = finding.ReviewNotes,
            DueDate = finding.DueDate,
            AssignedTo = finding.AssignedTo,
            ResolvedDate = finding.ResolvedDate,
            ResolvedBy = finding.ResolvedBy,
            ResolutionNotes = finding.ResolutionNotes,
            LastAnalysisDate = finding.LastAnalysisDate,
            AnalysisVersion = finding.AnalysisVersion,
            AnalysisModel = finding.AnalysisModel,
            CreatedAt = finding.Created,
            LastModified = finding.LastModified,
            RemediationTaskId = finding.RemediationTaskId,
            Control = new ControlDto
            {
                Id = finding.Control.Id,
                ControlReference = finding.Control.ControlReference,
                Title = finding.Control.Title,
                Description = finding.Control.Description,
                ImplementationGuidance = finding.Control.ImplementationGuidance
            },
            Framework = new FrameworkDto
            {
                Id = finding.Control.Framework.Id,
                Code = finding.Control.Framework.Code,
                Name = finding.Control.Framework.Name,
                Version = finding.Control.Framework.Version
            },
            Evidence = finding.Evidence.Select(e => new EvidenceDto
            {
                Id = e.Id,
                DocumentId = e.DocumentId,
                DocumentName = e.Document.FileName,
                EvidenceType = e.EvidenceType,
                Excerpt = e.Excerpt,
                PageReference = e.PageReference,
                RelevanceScore = e.RelevanceScore,
                Notes = e.Notes,
                IsManuallyAdded = e.IsManuallyAdded
            }).ToList(),
            Comments = finding.Comments
                .Where(c => c.ParentCommentId == null)
                .Select(c => MapCommentToDto(c))
                .OrderBy(c => c.CreatedAt)
                .ToList()
        };

        return Result<FindingDetailsDto>.Success(result);
    }

    private static CommentDto MapCommentToDto(FindingComment comment)
    {
        return new CommentDto
        {
            Id = comment.Id,
            ParentCommentId = comment.ParentCommentId,
            Content = comment.Content,
            CreatedBy = comment.CreatedBy ?? "Unknown",
            CreatedAt = comment.Created,
            IsEdited = comment.IsEdited,
            EditedDate = comment.EditedDate,
            IsResolutionComment = comment.IsResolutionComment,
            Mentions = string.IsNullOrWhiteSpace(comment.Mentions)
                ? new List<string>()
                : System.Text.Json.JsonSerializer.Deserialize<List<string>>(comment.Mentions) ?? new List<string>(),
            Replies = comment.Replies
                .Select(r => MapCommentToDto(r))
                .OrderBy(r => r.CreatedAt)
                .ToList()
        };
    }
}
