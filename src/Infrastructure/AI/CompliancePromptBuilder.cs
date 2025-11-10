using IgnaCheck.Application.Common.Models.AI;
using System.Text;

namespace IgnaCheck.Infrastructure.AI;

/// <summary>
/// Expert-level prompt engineering for compliance analysis.
/// Generates professional, structured prompts for high-quality AI-powered compliance assessments.
/// </summary>
public static class CompliancePromptBuilder
{
    /// <summary>
    /// Builds a comprehensive control analysis prompt for AI compliance assessment.
    /// This prompt is engineered to produce professional, audit-ready analysis reports.
    /// </summary>
    public static string BuildControlAnalysisPrompt(ControlAnalysisRequest request)
    {
        var prompt = new StringBuilder();

        // System context and role definition
        prompt.AppendLine("You are an expert compliance auditor and cybersecurity consultant with deep expertise in regulatory frameworks including ISO 27001, SOC 2, DORA, GDPR, PCI DSS, and other industry standards.");
        prompt.AppendLine();
        prompt.AppendLine("Your task is to conduct a thorough, professional compliance assessment by analyzing organizational documentation against a specific regulatory control.");
        prompt.AppendLine();

        // Analysis Framework
        prompt.AppendLine("═══════════════════════════════════════════════════════════");
        prompt.AppendLine("CONTROL TO ASSESS");
        prompt.AppendLine("═══════════════════════════════════════════════════════════");
        prompt.AppendLine();
        prompt.AppendLine($"Control Code: {request.ControlCode}");
        prompt.AppendLine($"Control Title: {request.ControlTitle}");
        prompt.AppendLine();
        prompt.AppendLine("Control Description:");
        prompt.AppendLine(request.ControlDescription);
        prompt.AppendLine();

        if (!string.IsNullOrWhiteSpace(request.ImplementationGuidance))
        {
            prompt.AppendLine("Implementation Guidance:");
            prompt.AppendLine(request.ImplementationGuidance);
            prompt.AppendLine();
        }

        // Document Evidence
        prompt.AppendLine("═══════════════════════════════════════════════════════════");
        prompt.AppendLine("ORGANIZATIONAL DOCUMENTATION PROVIDED");
        prompt.AppendLine("═══════════════════════════════════════════════════════════");
        prompt.AppendLine();

        if (!request.Documents.Any())
        {
            prompt.AppendLine("⚠️ NO DOCUMENTS PROVIDED");
            prompt.AppendLine("This indicates a critical compliance gap - no evidence exists for this control.");
        }
        else
        {
            for (int i = 0; i < request.Documents.Count; i++)
            {
                var doc = request.Documents[i];
                prompt.AppendLine($"Document {i + 1}: {doc.FileName}");
                prompt.AppendLine($"Type: {doc.MimeType}");

                if (doc.PageCount > 0)
                {
                    prompt.AppendLine($"Pages: {doc.PageCount}");
                }

                prompt.AppendLine();
                prompt.AppendLine("Content:");
                prompt.AppendLine("───────────────────────────────────────────────────────────");

                if (string.IsNullOrWhiteSpace(doc.Content))
                {
                    prompt.AppendLine("⚠️ [EMPTY OR UNREADABLE CONTENT]");
                }
                else
                {
                    // Truncate very long documents to avoid token limits
                    var content = doc.Content.Length > 15000
                        ? doc.Content.Substring(0, 15000) + "\n\n[... CONTENT TRUNCATED FOR LENGTH ...]"
                        : doc.Content;
                    prompt.AppendLine(content);
                }

                prompt.AppendLine("───────────────────────────────────────────────────────────");
                prompt.AppendLine();
            }
        }

        // Analysis Instructions
        prompt.AppendLine("═══════════════════════════════════════════════════════════");
        prompt.AppendLine("ANALYSIS INSTRUCTIONS");
        prompt.AppendLine("═══════════════════════════════════════════════════════════");
        prompt.AppendLine();
        prompt.AppendLine("Conduct a thorough compliance assessment following these professional standards:");
        prompt.AppendLine();
        prompt.AppendLine("1. EVIDENCE EVALUATION");
        prompt.AppendLine("   - Carefully review all provided documents");
        prompt.AppendLine("   - Identify specific evidence that addresses control requirements");
        prompt.AppendLine("   - Note exact excerpts and locations (page numbers if available)");
        prompt.AppendLine("   - Assess the quality, completeness, and recency of evidence");
        prompt.AppendLine();
        prompt.AppendLine("2. COMPLIANCE DETERMINATION");
        prompt.AppendLine("   - Compliant: All control requirements are fully met with strong evidence");
        prompt.AppendLine("   - PartiallyCompliant: Some requirements met, but gaps or weaknesses exist");
        prompt.AppendLine("   - NonCompliant: Control requirements are not met or evidence is missing");
        prompt.AppendLine();
        prompt.AppendLine("3. RISK ASSESSMENT");
        prompt.AppendLine("   - Critical: Immediate risk to security, data, operations, or regulatory standing");
        prompt.AppendLine("   - High: Significant risk requiring prompt remediation");
        prompt.AppendLine("   - Medium: Moderate risk that should be addressed in near term");
        prompt.AppendLine("   - Low: Minor gap with limited impact");
        prompt.AppendLine();
        prompt.AppendLine("4. GAP ANALYSIS");
        prompt.AppendLine("   - Identify specific missing elements or weaknesses");
        prompt.AppendLine("   - Explain why each gap creates risk");
        prompt.AppendLine("   - Reference specific control requirements not met");
        prompt.AppendLine();
        prompt.AppendLine("5. REMEDIATION GUIDANCE");
        prompt.AppendLine("   - Provide clear, actionable steps to achieve compliance");
        prompt.AppendLine("   - Prioritize recommendations by impact");
        prompt.AppendLine("   - Suggest specific documentation, processes, or controls to implement");
        prompt.AppendLine("   - Estimate reasonable effort required (in hours)");
        prompt.AppendLine();

        // Output Format
        prompt.AppendLine("═══════════════════════════════════════════════════════════");
        prompt.AppendLine("REQUIRED OUTPUT FORMAT");
        prompt.AppendLine("═══════════════════════════════════════════════════════════");
        prompt.AppendLine();
        prompt.AppendLine("Respond ONLY with a valid JSON object in this exact structure:");
        prompt.AppendLine();
        prompt.AppendLine("{");
        prompt.AppendLine("  \"status\": \"Compliant\" | \"PartiallyCompliant\" | \"NonCompliant\",");
        prompt.AppendLine("  \"riskLevel\": \"Critical\" | \"High\" | \"Medium\" | \"Low\",");
        prompt.AppendLine("  \"findingTitle\": \"Brief, professional title (max 120 chars)\",");
        prompt.AppendLine("  \"findingDescription\": \"Detailed analysis including: what was found, what's missing, why it matters, regulatory implications\",");
        prompt.AppendLine("  \"remediationGuidance\": \"Specific, actionable steps to achieve compliance\",");
        prompt.AppendLine("  \"confidenceScore\": 0.95,");
        prompt.AppendLine("  \"evidenceReferences\": [");
        prompt.AppendLine("    {");
        prompt.AppendLine("      \"documentId\": \"guid-from-Documents-array\",");
        prompt.AppendLine("      \"fileName\": \"document-name.pdf\",");
        prompt.AppendLine("      \"excerpt\": \"Exact quote from document (max 500 chars)\",");
        prompt.AppendLine("      \"pageReference\": \"Page 5, Section 3.2\",");
        prompt.AppendLine("      \"relevanceScore\": 0.9");
        prompt.AppendLine("    }");
        prompt.AppendLine("  ],");
        prompt.AppendLine("  \"missingElements\": [");
        prompt.AppendLine("    \"Specific requirement 1 not met\",");
        prompt.AppendLine("    \"Specific requirement 2 not met\"");
        prompt.AppendLine("  ],");
        prompt.AppendLine("  \"estimatedEffortHours\": 16");
        prompt.AppendLine("}");
        prompt.AppendLine();
        prompt.AppendLine("CRITICAL REQUIREMENTS:");
        prompt.AppendLine("- Response must be valid JSON only (no markdown, no additional text)");
        prompt.AppendLine("- Status must be one of: Compliant, PartiallyCompliant, NonCompliant");
        prompt.AppendLine("- RiskLevel must be one of: Critical, High, Medium, Low");
        prompt.AppendLine("- ConfidenceScore must be 0.0-1.0 (your confidence in the assessment)");
        prompt.AppendLine("- FindingDescription should be 200-1000 characters: comprehensive but concise");
        prompt.AppendLine("- Include at least 1 evidenceReference if documents were provided");
        prompt.AppendLine("- Be professional, objective, and audit-ready in tone");
        prompt.AppendLine("- Focus on facts and evidence, not assumptions");
        prompt.AppendLine();
        prompt.AppendLine("Begin your analysis now.");

        return prompt.ToString();
    }

    /// <summary>
    /// Builds a prompt for extracting relevant text excerpts from documents.
    /// </summary>
    public static string BuildExcerptExtractionPrompt(string documentContent, string controlDescription)
    {
        var prompt = new StringBuilder();

        prompt.AppendLine("You are a compliance documentation expert specializing in evidence extraction.");
        prompt.AppendLine();
        prompt.AppendLine("TASK: Extract the most relevant excerpts from the provided document that relate to the control requirement.");
        prompt.AppendLine();
        prompt.AppendLine("CONTROL REQUIREMENT:");
        prompt.AppendLine(controlDescription);
        prompt.AppendLine();
        prompt.AppendLine("DOCUMENT CONTENT:");
        prompt.AppendLine("───────────────────────────────────────────────────────────");

        // Truncate if too long
        var content = documentContent.Length > 10000
            ? documentContent.Substring(0, 10000) + "\n\n[... TRUNCATED ...]"
            : documentContent;

        prompt.AppendLine(content);
        prompt.AppendLine("───────────────────────────────────────────────────────────");
        prompt.AppendLine();
        prompt.AppendLine("Extract 1-5 most relevant excerpts. Respond with JSON:");
        prompt.AppendLine();
        prompt.AppendLine("[");
        prompt.AppendLine("  {");
        prompt.AppendLine("    \"excerpt\": \"Exact text from document\",");
        prompt.AppendLine("    \"pageReference\": \"Page/Section\",");
        prompt.AppendLine("    \"relevanceScore\": 0.95,");
        prompt.AppendLine("    \"relevanceReason\": \"Why this excerpt is relevant\"");
        prompt.AppendLine("  }");
        prompt.AppendLine("]");

        return prompt.ToString();
    }

    /// <summary>
    /// Builds a prompt for generating detailed remediation guidance.
    /// </summary>
    public static string BuildRemediationGuidancePrompt(RemediationRequest request)
    {
        var prompt = new StringBuilder();

        prompt.AppendLine("You are a compliance remediation consultant with expertise in implementing security controls and regulatory requirements.");
        prompt.AppendLine();
        prompt.AppendLine("CONTROL REQUIREMENT:");
        prompt.AppendLine(request.ControlDescription);
        prompt.AppendLine();
        prompt.AppendLine("IDENTIFIED GAP:");
        prompt.AppendLine(request.GapDescription);
        prompt.AppendLine();
        prompt.AppendLine("CURRENT STATUS:");
        prompt.AppendLine(request.CurrentStatus.ToString());
        prompt.AppendLine();
        prompt.AppendLine("TASK: Provide detailed, actionable remediation guidance.");
        prompt.AppendLine();
        prompt.AppendLine("Respond with JSON:");
        prompt.AppendLine("{");
        prompt.AppendLine("  \"summary\": \"Executive summary of remediation approach\",");
        prompt.AppendLine("  \"steps\": [");
        prompt.AppendLine("    {");
        prompt.AppendLine("      \"stepNumber\": 1,");
        prompt.AppendLine("      \"description\": \"Step description\",");
        prompt.AppendLine("      \"requiredActions\": [\"Action 1\", \"Action 2\"],");
        prompt.AppendLine("      \"estimatedHours\": 8,");
        prompt.AppendLine("      \"priority\": \"High\"");
        prompt.AppendLine("    }");
        prompt.AppendLine("  ],");
        prompt.AppendLine("  \"estimatedEffortHours\": 24,");
        prompt.AppendLine("  \"resources\": [\"Resource 1\", \"Resource 2\"],");
        prompt.AppendLine("  \"bestPractices\": \"Industry best practices and recommendations\"");
        prompt.AppendLine("}");

        return prompt.ToString();
    }
}
