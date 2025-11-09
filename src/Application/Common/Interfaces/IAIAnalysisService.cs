using IgnaCheck.Application.Common.Models.AI;

namespace IgnaCheck.Application.Common.Interfaces;

/// <summary>
/// Service for AI-powered compliance analysis.
/// Analyzes documents against compliance framework controls to identify gaps.
/// </summary>
public interface IAIAnalysisService
{
    /// <summary>
    /// Analyzes a document against a specific compliance control.
    /// </summary>
    /// <param name="request">Analysis request containing document content and control details</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Analysis result with compliance assessment and findings</returns>
    Task<ControlAnalysisResult> AnalyzeDocumentAgainstControlAsync(
        ControlAnalysisRequest request,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Analyzes multiple documents against a complete framework.
    /// </summary>
    /// <param name="request">Batch analysis request</param>
    /// <param name="progressCallback">Optional callback for progress updates</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Comprehensive framework analysis results</returns>
    Task<FrameworkAnalysisResult> AnalyzeDocumentsAgainstFrameworkAsync(
        FrameworkAnalysisRequest request,
        IProgress<AnalysisProgress>? progressCallback = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Extracts relevant text excerpts from a document for a specific control.
    /// </summary>
    /// <param name="documentContent">Full document content</param>
    /// <param name="controlDescription">Control description</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Relevant text excerpts with page references</returns>
    Task<List<TextExcerpt>> ExtractRelevantExcerptsAsync(
        string documentContent,
        string controlDescription,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Generates remediation guidance for a compliance gap.
    /// </summary>
    /// <param name="request">Remediation request</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>AI-generated remediation guidance</returns>
    Task<RemediationGuidance> GenerateRemediationGuidanceAsync(
        RemediationRequest request,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Calculates an overall compliance score based on findings.
    /// </summary>
    /// <param name="findings">List of findings</param>
    /// <returns>Compliance score (0-100)</returns>
    decimal CalculateComplianceScore(List<FindingSummary> findings);
}
