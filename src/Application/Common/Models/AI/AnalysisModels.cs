using IgnaCheck.Domain.Enums;

namespace IgnaCheck.Application.Common.Models.AI;

/// <summary>
/// Request to analyze a document against a specific compliance control.
/// </summary>
public record ControlAnalysisRequest
{
    public Guid ProjectId { get; init; }
    public Guid ControlId { get; init; }
    public string ControlCode { get; init; } = string.Empty;
    public string ControlTitle { get; init; } = string.Empty;
    public string ControlDescription { get; init; } = string.Empty;
    public string? ImplementationGuidance { get; init; }
    public List<DocumentContent> Documents { get; init; } = new();
}

/// <summary>
/// Document content for AI analysis.
/// </summary>
public record DocumentContent
{
    public Guid DocumentId { get; init; }
    public string FileName { get; init; } = string.Empty;
    public string Content { get; init; } = string.Empty;
    public string MimeType { get; init; } = string.Empty;
    public int PageCount { get; init; }
}

/// <summary>
/// Result of analyzing a document against a control.
/// </summary>
public record ControlAnalysisResult
{
    public Guid ControlId { get; init; }
    public ComplianceStatus Status { get; init; }
    public RiskLevel RiskLevel { get; init; }
    public string FindingTitle { get; init; } = string.Empty;
    public string FindingDescription { get; init; } = string.Empty;
    public string? RemediationGuidance { get; init; }
    public decimal ConfidenceScore { get; init; }
    public List<EvidenceReference> EvidenceReferences { get; init; } = new();
    public List<string> MissingElements { get; init; } = new();
    public decimal? EstimatedEffortHours { get; init; }
}

/// <summary>
/// Reference to evidence found in a document.
/// </summary>
public record EvidenceReference
{
    public Guid DocumentId { get; init; }
    public string DocumentName { get; init; } = string.Empty;
    public string Excerpt { get; init; } = string.Empty;
    public string? PageReference { get; init; }
    public decimal RelevanceScore { get; init; }
    public EvidenceType EvidenceType { get; init; }
}

/// <summary>
/// Request to analyze documents against an entire framework.
/// </summary>
public record FrameworkAnalysisRequest
{
    public Guid ProjectId { get; init; }
    public Guid FrameworkId { get; init; }
    public string FrameworkCode { get; init; } = string.Empty;
    public string FrameworkName { get; init; } = string.Empty;
    public List<ControlSummary> Controls { get; init; } = new();
    public List<DocumentContent> Documents { get; init; } = new();
    public AnalysisOptions Options { get; init; } = new();
}

/// <summary>
/// Summary of a control for analysis.
/// </summary>
public record ControlSummary
{
    public Guid ControlId { get; init; }
    public string ControlCode { get; init; } = string.Empty;
    public string Title { get; init; } = string.Empty;
    public string Description { get; init; } = string.Empty;
    public string? ImplementationGuidance { get; init; }
    public RiskLevel DefaultRiskLevel { get; init; }
    public bool IsMandatory { get; init; }
}

/// <summary>
/// Options for AI analysis.
/// </summary>
public record AnalysisOptions
{
    /// <summary>
    /// AI model to use (e.g., "gpt-4", "claude-3-sonnet").
    /// </summary>
    public string Model { get; init; } = "claude-3-sonnet-20240229";

    /// <summary>
    /// Temperature for AI responses (0.0 - 1.0).
    /// </summary>
    public decimal Temperature { get; init; } = 0.3m;

    /// <summary>
    /// Maximum tokens for AI response.
    /// </summary>
    public int MaxTokens { get; init; } = 4000;

    /// <summary>
    /// Skip analysis for controls already assessed.
    /// </summary>
    public bool SkipExistingFindings { get; init; } = false;

    /// <summary>
    /// Analyze only mandatory controls.
    /// </summary>
    public bool MandatoryControlsOnly { get; init; } = false;
}

/// <summary>
/// Result of analyzing documents against a framework.
/// </summary>
public record FrameworkAnalysisResult
{
    public Guid ProjectId { get; init; }
    public Guid FrameworkId { get; init; }
    public DateTime AnalysisStarted { get; init; }
    public DateTime AnalysisCompleted { get; init; }
    public TimeSpan Duration => AnalysisCompleted - AnalysisStarted;
    public int TotalControls { get; init; }
    public int ControlsAnalyzed { get; init; }
    public int FindingsCreated { get; init; }
    public List<ControlAnalysisResult> Results { get; init; } = new();
    public ComplianceSummary Summary { get; init; } = new();
    public string? ErrorMessage { get; init; }
}

/// <summary>
/// Summary of compliance status.
/// </summary>
public record ComplianceSummary
{
    public decimal OverallComplianceScore { get; init; }
    public int CompliantCount { get; init; }
    public int PartiallyCompliantCount { get; init; }
    public int NonCompliantCount { get; init; }
    public int NotAssessedCount { get; init; }
    public int CriticalFindings { get; init; }
    public int HighRiskFindings { get; init; }
    public int MediumRiskFindings { get; init; }
    public int LowRiskFindings { get; init; }
}

/// <summary>
/// Progress update for long-running analysis.
/// </summary>
public record AnalysisProgress
{
    public int TotalControls { get; init; }
    public int ControlsAnalyzed { get; init; }
    public int FindingsFound { get; init; }
    public string? CurrentControl { get; init; }
    public decimal PercentComplete => TotalControls > 0 ? (decimal)ControlsAnalyzed / TotalControls * 100 : 0;
    public DateTime StartTime { get; init; }
    public TimeSpan EstimatedTimeRemaining { get; init; }
}

/// <summary>
/// Text excerpt extracted from a document.
/// </summary>
public record TextExcerpt
{
    public string Text { get; init; } = string.Empty;
    public string? PageReference { get; init; }
    public decimal RelevanceScore { get; init; }
}

/// <summary>
/// Request for remediation guidance generation.
/// </summary>
public record RemediationRequest
{
    public Guid ControlId { get; init; }
    public string ControlDescription { get; init; } = string.Empty;
    public string GapDescription { get; init; } = string.Empty;
    public ComplianceStatus CurrentStatus { get; init; }
    public RiskLevel RiskLevel { get; init; }
    public string? OrganizationContext { get; init; }
}

/// <summary>
/// AI-generated remediation guidance.
/// </summary>
public record RemediationGuidance
{
    public string Summary { get; init; } = string.Empty;
    public List<RemediationStep> Steps { get; init; } = new();
    public decimal EstimatedEffortHours { get; init; }
    public List<string> Resources { get; init; } = new();
    public string? BestPractices { get; init; }
}

/// <summary>
/// Individual remediation step.
/// </summary>
public record RemediationStep
{
    public int StepNumber { get; init; }
    public string Description { get; init; } = string.Empty;
    public string? Implementation { get; init; }
    public List<string> RequiredActions { get; init; } = new();
}

/// <summary>
/// Summary of a finding for scoring.
/// </summary>
public record FindingSummary
{
    public ComplianceStatus Status { get; init; }
    public RiskLevel RiskLevel { get; init; }
    public bool IsMandatory { get; init; }
}
