using IgnaCheck.Application.Common.Interfaces;
using IgnaCheck.Domain.Enums;

namespace IgnaCheck.Application.Reports.Queries.GetExecutiveSummary;

/// <summary>
/// Query to get executive summary report for a project.
/// </summary>
public record GetExecutiveSummaryQuery : IRequest<Result<ExecutiveSummaryDto>>
{
    /// <summary>
    /// Project ID.
    /// </summary>
    public Guid ProjectId { get; init; }
}

/// <summary>
/// Executive summary report for management.
/// </summary>
public record ExecutiveSummaryDto
{
    public Guid ProjectId { get; init; }
    public string ProjectName { get; init; } = string.Empty;
    public string? ProjectDescription { get; init; }
    public DateTime ReportGeneratedDate { get; init; }
    public DateTime? LastAnalysisDate { get; init; }

    // Executive Summary
    public string ExecutiveSummary { get; init; } = string.Empty;

    // Key Metrics
    public decimal OverallComplianceScore { get; init; }
    public int TotalFrameworks { get; init; }
    public int TotalControls { get; init; }
    public int TotalFindings { get; init; }
    public int CriticalFindings { get; init; }
    public int OpenCriticalFindings { get; init; }

    // Compliance Status Summary
    public ComplianceStatusSummaryDto ComplianceStatus { get; init; } = new();

    // Framework Summary
    public List<FrameworkSummaryDto> Frameworks { get; init; } = new();

    // Top Risks
    public List<TopRiskDto> TopRisks { get; init; } = new();

    // Recommendations
    public List<string> KeyRecommendations { get; init; } = new();

    // Progress Metrics
    public ProgressMetricsDto Progress { get; init; } = new();
}

public record ComplianceStatusSummaryDto
{
    public int Compliant { get; init; }
    public int PartiallyCompliant { get; init; }
    public int NonCompliant { get; init; }
    public int NotAssessed { get; init; }
}

public record FrameworkSummaryDto
{
    public string FrameworkName { get; init; } = string.Empty;
    public string FrameworkCode { get; init; } = string.Empty;
    public decimal ComplianceScore { get; init; }
    public int TotalControls { get; init; }
    public int CompliantControls { get; init; }
    public int CriticalFindings { get; init; }
}

public record TopRiskDto
{
    public string Title { get; init; } = string.Empty;
    public string FrameworkName { get; init; } = string.Empty;
    public string ControlCode { get; init; } = string.Empty;
    public RiskLevel RiskLevel { get; init; }
    public string? RemediationGuidance { get; init; }
    public string? AssignedTo { get; init; }
    public DateTime? DueDate { get; init; }
}

public record ProgressMetricsDto
{
    public int TotalFindings { get; init; }
    public int ResolvedFindings { get; init; }
    public int InProgressFindings { get; init; }
    public int OpenFindings { get; init; }
    public decimal ResolutionRate { get; init; }
}

/// <summary>
/// Handler for GetExecutiveSummaryQuery.
/// </summary>
public class GetExecutiveSummaryQueryHandler : IRequestHandler<GetExecutiveSummaryQuery, Result<ExecutiveSummaryDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly IUser _currentUser;

    public GetExecutiveSummaryQueryHandler(
        IApplicationDbContext context,
        IUser currentUser)
    {
        _context = context;
        _currentUser = currentUser;
    }

    public async Task<Result<ExecutiveSummaryDto>> Handle(GetExecutiveSummaryQuery request, CancellationToken cancellationToken)
    {
        // Ensure user is authenticated
        if (string.IsNullOrEmpty(_currentUser.Id))
        {
            return Result<ExecutiveSummaryDto>.Failure(new[] { "User must be authenticated." });
        }

        // Verify project exists and user has access
        var project = await _context.Projects
            .Include(p => p.ProjectMembers)
            .Include(p => p.ProjectFrameworks)
            .ThenInclude(pf => pf.Framework)
            .FirstOrDefaultAsync(p => p.Id == request.ProjectId, cancellationToken);

        if (project == null)
        {
            return Result<ExecutiveSummaryDto>.Failure(new[] { "Project not found." });
        }

        // Check if user is a member of the project
        var isMember = project.ProjectMembers.Any(pm => pm.UserId == _currentUser.Id && pm.IsActive);
        if (!isMember)
        {
            return Result<ExecutiveSummaryDto>.Failure(new[] { "Access denied. You are not a member of this project." });
        }

        // Get all findings for the project
        var findings = await _context.ComplianceFindings
            .Include(f => f.Control)
            .ThenInclude(c => c.Framework)
            .Where(f => f.ProjectId == request.ProjectId)
            .ToListAsync(cancellationToken);

        // Calculate key metrics
        var totalControls = findings.Count;
        var statusGroups = findings.GroupBy(f => f.Status).ToDictionary(g => g.Key, g => g.Count());
        var compliantControls = statusGroups.GetValueOrDefault(ComplianceStatus.Compliant, 0);
        var partiallyCompliant = statusGroups.GetValueOrDefault(ComplianceStatus.PartiallyCompliant, 0);
        var nonCompliant = statusGroups.GetValueOrDefault(ComplianceStatus.NonCompliant, 0);
        var notAssessed = statusGroups.GetValueOrDefault(ComplianceStatus.NotAssessed, 0);

        var overallScore = totalControls > 0
            ? (decimal)(compliantControls * 100 + partiallyCompliant * 50) / totalControls
            : 0;

        var criticalFindings = findings.Count(f => f.RiskLevel == RiskLevel.Critical);
        var openCriticalFindings = findings.Count(f => f.RiskLevel == RiskLevel.Critical && f.WorkflowStatus == FindingWorkflowStatus.Open);

        // Last analysis date
        var lastAnalysisDate = findings
            .Where(f => f.LastAnalysisDate.HasValue)
            .Select(f => f.LastAnalysisDate!.Value)
            .DefaultIfEmpty()
            .Max();

        // Framework summary
        var frameworkSummary = findings
            .GroupBy(f => new { f.Control.FrameworkId, f.Control.Framework.Name, f.Control.Framework.Code })
            .Select(g =>
            {
                var frameworkFindings = g.ToList();
                var frameworkCompliant = frameworkFindings.Count(f => f.Status == ComplianceStatus.Compliant);
                var frameworkPartial = frameworkFindings.Count(f => f.Status == ComplianceStatus.PartiallyCompliant);
                var frameworkTotal = frameworkFindings.Count;
                var frameworkScore = frameworkTotal > 0
                    ? (decimal)(frameworkCompliant * 100 + frameworkPartial * 50) / frameworkTotal
                    : 0;

                return new FrameworkSummaryDto
                {
                    FrameworkName = g.Key.Name,
                    FrameworkCode = g.Key.Code,
                    ComplianceScore = frameworkScore,
                    TotalControls = frameworkTotal,
                    CompliantControls = frameworkCompliant,
                    CriticalFindings = frameworkFindings.Count(f => f.RiskLevel == RiskLevel.Critical)
                };
            })
            .OrderBy(f => f.ComplianceScore)
            .ToList();

        // Top risks (top 5 critical/high findings that are open)
        var topRisks = findings
            .Where(f => (f.RiskLevel == RiskLevel.Critical || f.RiskLevel == RiskLevel.High) &&
                       (f.WorkflowStatus == FindingWorkflowStatus.Open || f.WorkflowStatus == FindingWorkflowStatus.InProgress))
            .OrderByDescending(f => f.RiskLevel)
            .ThenBy(f => f.DueDate ?? DateTime.MaxValue)
            .Take(5)
            .Select(f => new TopRiskDto
            {
                Title = f.Title,
                FrameworkName = f.Control.Framework.Name,
                ControlCode = f.Control.ControlCode,
                RiskLevel = f.RiskLevel,
                RemediationGuidance = f.RemediationGuidance,
                AssignedTo = f.AssignedTo,
                DueDate = f.DueDate
            })
            .ToList();

        // Progress metrics
        var totalFindings = findings.Count;
        var resolvedFindings = findings.Count(f => f.WorkflowStatus == FindingWorkflowStatus.Resolved);
        var inProgressFindings = findings.Count(f => f.WorkflowStatus == FindingWorkflowStatus.InProgress);
        var openFindings = findings.Count(f => f.WorkflowStatus == FindingWorkflowStatus.Open);
        var resolutionRate = totalFindings > 0 ? (decimal)resolvedFindings / totalFindings * 100 : 0;

        // Generate executive summary text
        var executiveSummary = GenerateExecutiveSummaryText(
            project.Name,
            overallScore,
            totalControls,
            compliantControls,
            criticalFindings,
            openCriticalFindings,
            resolutionRate
        );

        // Generate key recommendations
        var keyRecommendations = GenerateKeyRecommendations(
            criticalFindings,
            openCriticalFindings,
            overallScore,
            resolutionRate,
            frameworkSummary
        );

        var summary = new ExecutiveSummaryDto
        {
            ProjectId = project.Id,
            ProjectName = project.Name,
            ProjectDescription = project.Description,
            ReportGeneratedDate = DateTime.UtcNow,
            LastAnalysisDate = lastAnalysisDate == DateTime.MinValue ? null : lastAnalysisDate,
            ExecutiveSummary = executiveSummary,
            OverallComplianceScore = overallScore,
            TotalFrameworks = frameworkSummary.Count,
            TotalControls = totalControls,
            TotalFindings = totalFindings,
            CriticalFindings = criticalFindings,
            OpenCriticalFindings = openCriticalFindings,
            ComplianceStatus = new ComplianceStatusSummaryDto
            {
                Compliant = compliantControls,
                PartiallyCompliant = partiallyCompliant,
                NonCompliant = nonCompliant,
                NotAssessed = notAssessed
            },
            Frameworks = frameworkSummary,
            TopRisks = topRisks,
            KeyRecommendations = keyRecommendations,
            Progress = new ProgressMetricsDto
            {
                TotalFindings = totalFindings,
                ResolvedFindings = resolvedFindings,
                InProgressFindings = inProgressFindings,
                OpenFindings = openFindings,
                ResolutionRate = resolutionRate
            }
        };

        return Result<ExecutiveSummaryDto>.Success(summary);
    }

    private static string GenerateExecutiveSummaryText(
        string projectName,
        decimal overallScore,
        int totalControls,
        int compliantControls,
        int criticalFindings,
        int openCriticalFindings,
        decimal resolutionRate)
    {
        var complianceLevel = overallScore >= 90 ? "high" : overallScore >= 70 ? "moderate" : "low";

        return $"The {projectName} compliance assessment shows a {complianceLevel} level of compliance with an overall score of {overallScore:F1}%. " +
               $"Of the {totalControls} controls assessed, {compliantControls} are fully compliant. " +
               $"{(criticalFindings > 0 ? $"There are {criticalFindings} critical findings identified, with {openCriticalFindings} currently open and requiring immediate attention. " : "No critical findings were identified. ")}" +
               $"The current remediation progress shows a {resolutionRate:F1}% resolution rate. " +
               $"{(overallScore < 70 ? "Significant effort is required to achieve compliance." : "The project is on track to achieve compliance with continued focus on remediation.")}";
    }

    private static List<string> GenerateKeyRecommendations(
        int criticalFindings,
        int openCriticalFindings,
        decimal overallScore,
        decimal resolutionRate,
        List<FrameworkSummaryDto> frameworks)
    {
        var recommendations = new List<string>();

        if (openCriticalFindings > 0)
        {
            recommendations.Add($"Prioritize immediate remediation of {openCriticalFindings} open critical findings to reduce organizational risk.");
        }

        if (overallScore < 70)
        {
            recommendations.Add("Implement a comprehensive remediation plan to address significant compliance gaps identified across multiple controls.");
        }

        if (resolutionRate < 50)
        {
            recommendations.Add("Accelerate remediation efforts to improve the finding resolution rate and demonstrate progress toward compliance.");
        }

        var lowScoreFrameworks = frameworks.Where(f => f.ComplianceScore < 60).ToList();
        if (lowScoreFrameworks.Any())
        {
            recommendations.Add($"Focus resources on improving compliance with {string.Join(", ", lowScoreFrameworks.Select(f => f.FrameworkCode))} frameworks, which are currently below acceptable thresholds.");
        }

        if (criticalFindings > 0)
        {
            recommendations.Add("Establish regular executive reviews of critical findings to ensure appropriate oversight and resource allocation.");
        }

        if (!recommendations.Any())
        {
            recommendations.Add("Continue current remediation efforts and maintain strong compliance posture through regular assessments and proactive control monitoring.");
        }

        return recommendations;
    }
}
