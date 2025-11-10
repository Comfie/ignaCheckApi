using IgnaCheck.Application.Common.Interfaces;
using IgnaCheck.Application.Common.Models.AI;
using IgnaCheck.Domain.Enums;
using IgnaCheck.Infrastructure.AI;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Text.Json;
using System.Text.RegularExpressions;
using Anthropic.SDK;
using Anthropic.SDK.Constants;
using Anthropic.SDK.Messaging;
using Azure.AI.OpenAI;
using OpenAI.Chat;

namespace IgnaCheck.Infrastructure.Services;

/// <summary>
/// AI-powered compliance analysis service.
/// Supports both Anthropic Claude and OpenAI ChatGPT for compliance analysis.
/// </summary>
public class AIAnalysisService : IAIAnalysisService
{
    private readonly ILogger<AIAnalysisService> _logger;
    private readonly AIConfiguration _config;
    private readonly AnthropicClient? _anthropicClient;
    private readonly AzureOpenAIClient? _openAIClient;
    private readonly IFileStorageService _fileStorageService;
    private readonly IDocumentParsingService _documentParsingService;

    public AIAnalysisService(
        ILogger<AIAnalysisService> logger,
        IOptions<AIConfiguration> config,
        IFileStorageService fileStorageService,
        IDocumentParsingService documentParsingService)
    {
        _logger = logger;
        _config = config.Value;
        _fileStorageService = fileStorageService;
        _documentParsingService = documentParsingService;

        // Initialize AI clients based on configuration
        try
        {
            if (_config.Provider == AIProvider.Claude && !string.IsNullOrEmpty(_config.Claude.ApiKey))
            {
                _anthropicClient = new AnthropicClient(new APIAuthentication(_config.Claude.ApiKey));
                _logger.LogInformation("Initialized Anthropic Claude client with model: {Model}", _config.Claude.Model);
            }

            if ((_config.Provider == AIProvider.OpenAI || _config.EnableFallback) && !string.IsNullOrEmpty(_config.OpenAI.ApiKey))
            {
                if (_config.OpenAI.UseAzure && !string.IsNullOrEmpty(_config.OpenAI.AzureEndpoint))
                {
                    _openAIClient = new AzureOpenAIClient(new Uri(_config.OpenAI.AzureEndpoint), new Azure.AzureKeyCredential(_config.OpenAI.ApiKey));
                    _logger.LogInformation("Initialized Azure OpenAI client with model: {Model}", _config.OpenAI.Model);
                }
                else
                {
                    _openAIClient = new AzureOpenAIClient(new Uri("https://api.openai.com/v1"), new Azure.AzureKeyCredential(_config.OpenAI.ApiKey));
                    _logger.LogInformation("Initialized OpenAI client with model: {Model}", _config.OpenAI.Model);
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to initialize AI clients. Check configuration.");
        }
    }

    public async Task<ControlAnalysisResult> AnalyzeDocumentAgainstControlAsync(
        ControlAnalysisRequest request,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Analyzing control {ControlCode} against {DocumentCount} documents",
            request.ControlCode, request.Documents.Count);

        try
        {
            // TODO: Replace with actual LLM call
            // For now, return a structured response showing the expected format
            var result = await AnalyzeWithLLMAsync(request, cancellationToken);

            _logger.LogInformation("Analysis complete for control {ControlCode}. Status: {Status}, Confidence: {Confidence}",
                request.ControlCode, result.Status, result.ConfidenceScore);

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error analyzing control {ControlCode}", request.ControlCode);
            throw;
        }
    }

    public async Task<FrameworkAnalysisResult> AnalyzeDocumentsAgainstFrameworkAsync(
        FrameworkAnalysisRequest request,
        IProgress<AnalysisProgress>? progressCallback = null,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Starting framework analysis for {FrameworkCode} with {ControlCount} controls and {DocumentCount} documents",
            request.FrameworkCode, request.Controls.Count, request.Documents.Count);

        var startTime = DateTime.UtcNow;
        var results = new List<ControlAnalysisResult>();
        var controlsAnalyzed = 0;

        try
        {
            foreach (var control in request.Controls)
            {
                // Check cancellation
                cancellationToken.ThrowIfCancellationRequested();

                // Skip if requested
                if (request.Options.SkipExistingFindings)
                {
                    // TODO: Check if finding already exists for this control
                }

                if (request.Options.MandatoryControlsOnly && !control.IsMandatory)
                {
                    continue;
                }

                // Analyze control
                var controlRequest = new ControlAnalysisRequest
                {
                    ProjectId = request.ProjectId,
                    ControlId = control.ControlId,
                    ControlCode = control.ControlCode,
                    ControlTitle = control.Title,
                    ControlDescription = control.Description,
                    ImplementationGuidance = control.ImplementationGuidance,
                    Documents = request.Documents
                };

                var result = await AnalyzeDocumentAgainstControlAsync(controlRequest, cancellationToken);
                results.Add(result);
                controlsAnalyzed++;

                // Report progress
                if (progressCallback != null)
                {
                    var elapsed = DateTime.UtcNow - startTime;
                    var avgTimePerControl = elapsed / controlsAnalyzed;
                    var remainingControls = request.Controls.Count - controlsAnalyzed;
                    var estimatedTimeRemaining = TimeSpan.FromTicks(avgTimePerControl.Ticks * remainingControls);

                    var progress = new AnalysisProgress
                    {
                        TotalControls = request.Controls.Count,
                        ControlsAnalyzed = controlsAnalyzed,
                        FindingsFound = results.Count(r => r.Status != ComplianceStatus.Compliant),
                        CurrentControl = $"{control.ControlCode}: {control.Title}",
                        StartTime = startTime,
                        EstimatedTimeRemaining = estimatedTimeRemaining
                    };

                    progressCallback.Report(progress);
                }

                // Rate limiting to avoid API throttling
                await Task.Delay(TimeSpan.FromMilliseconds(500), cancellationToken);
            }

            var endTime = DateTime.UtcNow;
            var summary = CalculateSummary(results);

            return new FrameworkAnalysisResult
            {
                ProjectId = request.ProjectId,
                FrameworkId = request.FrameworkId,
                AnalysisStarted = startTime,
                AnalysisCompleted = endTime,
                TotalControls = request.Controls.Count,
                ControlsAnalyzed = controlsAnalyzed,
                FindingsCreated = results.Count(r => r.Status != ComplianceStatus.Compliant),
                Results = results,
                Summary = summary
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during framework analysis for {FrameworkCode}", request.FrameworkCode);

            return new FrameworkAnalysisResult
            {
                ProjectId = request.ProjectId,
                FrameworkId = request.FrameworkId,
                AnalysisStarted = startTime,
                AnalysisCompleted = DateTime.UtcNow,
                TotalControls = request.Controls.Count,
                ControlsAnalyzed = controlsAnalyzed,
                FindingsCreated = 0,
                Results = results,
                Summary = CalculateSummary(results),
                ErrorMessage = ex.Message
            };
        }
    }

    public async Task<List<TextExcerpt>> ExtractRelevantExcerptsAsync(
        string documentContent,
        string controlDescription,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Extracting relevant excerpts for control");

        if (string.IsNullOrWhiteSpace(documentContent) || string.IsNullOrWhiteSpace(controlDescription))
        {
            return new List<TextExcerpt>();
        }

        try
        {
            var prompt = CompliancePromptBuilder.BuildExcerptExtractionPrompt(documentContent, controlDescription);

            string? responseText = null;

            if (_config.Provider == AIProvider.Claude && _anthropicClient != null)
            {
                responseText = await CallClaudeAsync(prompt, cancellationToken);
            }
            else if (_config.Provider == AIProvider.OpenAI && _openAIClient != null)
            {
                responseText = await CallOpenAIAsync(prompt, cancellationToken);
            }

            if (string.IsNullOrWhiteSpace(responseText))
            {
                return new List<TextExcerpt>();
            }

            // Clean and parse JSON response
            var cleanedResponse = responseText.Trim();
            if (cleanedResponse.StartsWith("```json"))
            {
                cleanedResponse = cleanedResponse.Substring(7);
            }
            else if (cleanedResponse.StartsWith("```"))
            {
                cleanedResponse = cleanedResponse.Substring(3);
            }
            if (cleanedResponse.EndsWith("```"))
            {
                cleanedResponse = cleanedResponse.Substring(0, cleanedResponse.Length - 3);
            }

            var excerpts = JsonSerializer.Deserialize<List<TextExcerpt>>(cleanedResponse.Trim(), new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
            return excerpts ?? new List<TextExcerpt>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to extract relevant excerpts");
            return new List<TextExcerpt>();
        }
    }

    public async Task<RemediationGuidance> GenerateRemediationGuidanceAsync(
        RemediationRequest request,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Generating remediation guidance for control {ControlId}", request.ControlId);

        try
        {
            var prompt = CompliancePromptBuilder.BuildRemediationGuidancePrompt(request);

            string? responseText = null;

            if (_config.Provider == AIProvider.Claude && _anthropicClient != null)
            {
                responseText = await CallClaudeAsync(prompt, cancellationToken);
            }
            else if (_config.Provider == AIProvider.OpenAI && _openAIClient != null)
            {
                responseText = await CallOpenAIAsync(prompt, cancellationToken);
            }

            if (string.IsNullOrWhiteSpace(responseText))
            {
                throw new InvalidOperationException("AI returned empty response");
            }

            // Clean and parse JSON response
            var cleanedResponse = responseText.Trim();
            if (cleanedResponse.StartsWith("```json"))
            {
                cleanedResponse = cleanedResponse.Substring(7);
            }
            else if (cleanedResponse.StartsWith("```"))
            {
                cleanedResponse = cleanedResponse.Substring(3);
            }
            if (cleanedResponse.EndsWith("```"))
            {
                cleanedResponse = cleanedResponse.Substring(0, cleanedResponse.Length - 3);
            }

            var guidance = JsonSerializer.Deserialize<RemediationGuidance>(cleanedResponse.Trim(), new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
            return guidance ?? throw new InvalidOperationException("Failed to parse remediation guidance");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to generate remediation guidance");

            // Return fallback guidance
            return new RemediationGuidance
            {
                Summary = "Remediation guidance generation failed. Manual review required.",
                Steps = new List<RemediationStep>
                {
                    new() { StepNumber = 1, Description = "Review control requirements", RequiredActions = new List<string> { "Analyze control description", "Identify gaps" } },
                    new() { StepNumber = 2, Description = "Implement necessary controls", RequiredActions = new List<string> { "Document procedures", "Configure systems" } },
                    new() { StepNumber = 3, Description = "Validate implementation", RequiredActions = new List<string> { "Test controls", "Gather evidence" } }
                },
                EstimatedEffortHours = 8,
                Resources = new List<string> { "Framework documentation", "Industry best practices" },
                BestPractices = "Ensure all changes are documented and approved."
            };
        }
    }

    public decimal CalculateComplianceScore(List<FindingSummary> findings)
    {
        if (!findings.Any())
            return 100m;

        var totalWeight = 0m;
        var achievedWeight = 0m;

        foreach (var finding in findings)
        {
            // Weight by mandatory status and risk level
            var baseWeight = finding.IsMandatory ? 2m : 1m;
            var riskMultiplier = finding.RiskLevel switch
            {
                RiskLevel.Critical => 4m,
                RiskLevel.High => 3m,
                RiskLevel.Medium => 2m,
                RiskLevel.Low => 1m,
                _ => 1m
            };

            var weight = baseWeight * riskMultiplier;
            totalWeight += weight;

            // Calculate achieved based on status
            var achievedPercentage = finding.Status switch
            {
                ComplianceStatus.Compliant => 1m,
                ComplianceStatus.PartiallyCompliant => 0.5m,
                ComplianceStatus.NonCompliant => 0m,
                ComplianceStatus.NotApplicable => 1m, // Don't penalize N/A
                _ => 0m
            };

            achievedWeight += weight * achievedPercentage;
        }

        return totalWeight > 0 ? Math.Round((achievedWeight / totalWeight) * 100, 2) : 100m;
    }

    // Private helper methods

    private async Task<ControlAnalysisResult> AnalyzeWithLLMAsync(
        ControlAnalysisRequest request,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation("Starting LLM analysis for control {ControlCode}", request.ControlCode);

        // Build professional compliance analysis prompt
        var prompt = CompliancePromptBuilder.BuildControlAnalysisPrompt(request);

        string? responseText = null;

        try
        {
            // Try primary provider
            if (_config.Provider == AIProvider.Claude && _anthropicClient != null)
            {
                responseText = await CallClaudeAsync(prompt, cancellationToken);
            }
            else if (_config.Provider == AIProvider.OpenAI && _openAIClient != null)
            {
                responseText = await CallOpenAIAsync(prompt, cancellationToken);
            }
            else
            {
                throw new InvalidOperationException("No AI provider configured or API key missing.");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Primary AI provider failed for control {ControlCode}", request.ControlCode);

            // Try fallback if enabled
            if (_config.EnableFallback)
            {
                try
                {
                    if (_config.Provider == AIProvider.Claude && _openAIClient != null)
                    {
                        _logger.LogInformation("Falling back to OpenAI for control {ControlCode}", request.ControlCode);
                        responseText = await CallOpenAIAsync(prompt, cancellationToken);
                    }
                    else if (_config.Provider == AIProvider.OpenAI && _anthropicClient != null)
                    {
                        _logger.LogInformation("Falling back to Claude for control {ControlCode}", request.ControlCode);
                        responseText = await CallClaudeAsync(prompt, cancellationToken);
                    }
                }
                catch (Exception fallbackEx)
                {
                    _logger.LogError(fallbackEx, "Fallback AI provider also failed for control {ControlCode}", request.ControlCode);
                    throw new InvalidOperationException("Both primary and fallback AI providers failed.", fallbackEx);
                }
            }
            else
            {
                throw;
            }
        }

        if (string.IsNullOrWhiteSpace(responseText))
        {
            throw new InvalidOperationException("AI provider returned empty response.");
        }

        // Parse AI response into structured result
        var result = ParseAIResponse(responseText, request);

        _logger.LogInformation("LLM analysis complete for control {ControlCode}. Status: {Status}, Confidence: {Confidence}",
            request.ControlCode, result.Status, result.ConfidenceScore);

        return result;
    }

    /// <summary>
    /// Calls Anthropic Claude API for compliance analysis.
    /// </summary>
    private async Task<string> CallClaudeAsync(string prompt, CancellationToken cancellationToken)
    {
        if (_anthropicClient == null)
        {
            throw new InvalidOperationException("Claude client not initialized.");
        }

        _logger.LogInformation("Calling Anthropic Claude API with model: {Model}", _config.Claude.Model);

        var messages = new List<Message>
        {
            new Message(RoleType.User, prompt)
        };

        var parameters = new MessageParameters
        {
            Messages = messages,
            MaxTokens = _config.MaxTokens,
            Model = _config.Claude.Model,
            Temperature = (decimal)_config.Temperature,
            Stream = false
        };

        var response = await _anthropicClient.Messages.GetClaudeMessageAsync(parameters, cancellationToken);

        if (response?.Content == null || !response.Content.Any())
        {
            throw new InvalidOperationException("Claude returned no content.");
        }

        // Extract text from response
        var textContent = response.Content.FirstOrDefault(c => c is TextContent) as TextContent;
        if (textContent == null || string.IsNullOrWhiteSpace(textContent.Text))
        {
            throw new InvalidOperationException("Claude response contains no text content.");
        }

        _logger.LogInformation("Claude API call successful. Response length: {Length} chars", textContent.Text.Length);

        return textContent.Text;
    }

    /// <summary>
    /// Calls OpenAI ChatGPT API for compliance analysis.
    /// </summary>
    private async Task<string> CallOpenAIAsync(string prompt, CancellationToken cancellationToken)
    {
        if (_openAIClient == null)
        {
            throw new InvalidOperationException("OpenAI client not initialized.");
        }

        _logger.LogInformation("Calling OpenAI API with model: {Model}", _config.OpenAI.Model);

        var chatClient = _openAIClient.GetChatClient(_config.OpenAI.Model);

        var messages = new List<ChatMessage>
        {
            new SystemChatMessage("You are an expert compliance auditor and cybersecurity consultant."),
            new UserChatMessage(prompt)
        };

        var completionOptions = new ChatCompletionOptions
        {
            MaxOutputTokenCount = _config.MaxTokens,
            Temperature = (float)_config.Temperature
        };

        var response = await chatClient.CompleteChatAsync(messages, completionOptions, cancellationToken);

        if (response?.Value?.Content == null || response.Value.Content.Count == 0)
        {
            throw new InvalidOperationException("OpenAI returned no content.");
        }

        var content = response.Value.Content[0].Text;

        if (string.IsNullOrWhiteSpace(content))
        {
            throw new InvalidOperationException("OpenAI response contains no text content.");
        }

        _logger.LogInformation("OpenAI API call successful. Response length: {Length} chars", content.Length);

        return content;
    }

    /// <summary>
    /// Parses AI response JSON into structured ControlAnalysisResult.
    /// </summary>
    private ControlAnalysisResult ParseAIResponse(string responseText, ControlAnalysisRequest request)
    {
        try
        {
            // Clean response - AI might wrap JSON in markdown code blocks
            var cleanedResponse = responseText.Trim();

            // Remove markdown code blocks if present
            if (cleanedResponse.StartsWith("```json"))
            {
                cleanedResponse = cleanedResponse.Substring(7);
            }
            else if (cleanedResponse.StartsWith("```"))
            {
                cleanedResponse = cleanedResponse.Substring(3);
            }

            if (cleanedResponse.EndsWith("```"))
            {
                cleanedResponse = cleanedResponse.Substring(0, cleanedResponse.Length - 3);
            }

            cleanedResponse = cleanedResponse.Trim();

            // Parse JSON
            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            };

            var aiResponse = JsonSerializer.Deserialize<AIAnalysisResponse>(cleanedResponse, options);

            if (aiResponse == null)
            {
                throw new InvalidOperationException("Failed to deserialize AI response.");
            }

            // Map to ControlAnalysisResult
            return new ControlAnalysisResult
            {
                ControlId = request.ControlId,
                Status = ParseComplianceStatus(aiResponse.Status),
                RiskLevel = ParseRiskLevel(aiResponse.RiskLevel),
                FindingTitle = aiResponse.FindingTitle ?? $"Analysis for {request.ControlCode}",
                FindingDescription = aiResponse.FindingDescription ?? "No description provided.",
                RemediationGuidance = aiResponse.RemediationGuidance ?? "No guidance provided.",
                ConfidenceScore = (decimal)aiResponse.ConfidenceScore,
                EvidenceReferences = aiResponse.EvidenceReferences?.Select(er => new EvidenceReference
                {
                    DocumentId = Guid.TryParse(er.DocumentId, out var docId) ? docId : Guid.Empty,
                    FileName = er.FileName ?? string.Empty,
                    Excerpt = er.Excerpt ?? string.Empty,
                    PageReference = er.PageReference,
                    RelevanceScore = (decimal?)er.RelevanceScore ?? 0.5m
                }).ToList() ?? new List<EvidenceReference>(),
                MissingElements = aiResponse.MissingElements ?? new List<string>(),
                EstimatedEffortHours = aiResponse.EstimatedEffortHours
            };
        }
        catch (JsonException ex)
        {
            _logger.LogError(ex, "Failed to parse AI response JSON: {Response}", responseText);

            // Return error result
            return new ControlAnalysisResult
            {
                ControlId = request.ControlId,
                Status = ComplianceStatus.NotAssessed,
                RiskLevel = RiskLevel.High,
                FindingTitle = $"Analysis Error for {request.ControlCode}",
                FindingDescription = $"AI returned invalid response format. Error: {ex.Message}",
                RemediationGuidance = "Manual review required. Check AI configuration and prompts.",
                ConfidenceScore = 0m,
                EvidenceReferences = new List<EvidenceReference>(),
                MissingElements = new List<string> { "AI response parsing failed" },
                EstimatedEffortHours = null
            };
        }
    }

    private ComplianceStatus ParseComplianceStatus(string? status)
    {
        return status?.ToLowerInvariant() switch
        {
            "compliant" => ComplianceStatus.Compliant,
            "partiallycompliant" => ComplianceStatus.PartiallyCompliant,
            "noncompliant" => ComplianceStatus.NonCompliant,
            "notapplicable" => ComplianceStatus.NotApplicable,
            _ => ComplianceStatus.NotAssessed
        };
    }

    private RiskLevel ParseRiskLevel(string? riskLevel)
    {
        return riskLevel?.ToLowerInvariant() switch
        {
            "critical" => RiskLevel.Critical,
            "high" => RiskLevel.High,
            "medium" => RiskLevel.Medium,
            "low" => RiskLevel.Low,
            _ => RiskLevel.Medium
        };
    }

    /// <summary>
    /// Internal DTO for deserializing AI response JSON.
    /// </summary>
    private class AIAnalysisResponse
    {
        public string? Status { get; set; }
        public string? RiskLevel { get; set; }
        public string? FindingTitle { get; set; }
        public string? FindingDescription { get; set; }
        public string? RemediationGuidance { get; set; }
        public double ConfidenceScore { get; set; }
        public List<AIEvidenceReference>? EvidenceReferences { get; set; }
        public List<string>? MissingElements { get; set; }
        public int? EstimatedEffortHours { get; set; }
    }

    private class AIEvidenceReference
    {
        public string? DocumentId { get; set; }
        public string? FileName { get; set; }
        public string? Excerpt { get; set; }
        public string? PageReference { get; set; }
        public double? RelevanceScore { get; set; }
    }

    private ComplianceSummary CalculateSummary(List<ControlAnalysisResult> results)
    {
        if (!results.Any())
        {
            return new ComplianceSummary();
        }

        var findings = results.Select(r => new FindingSummary
        {
            Status = r.Status,
            RiskLevel = r.RiskLevel,
            IsMandatory = true // TODO: Get from control
        }).ToList();

        return new ComplianceSummary
        {
            OverallComplianceScore = CalculateComplianceScore(findings),
            CompliantCount = results.Count(r => r.Status == ComplianceStatus.Compliant),
            PartiallyCompliantCount = results.Count(r => r.Status == ComplianceStatus.PartiallyCompliant),
            NonCompliantCount = results.Count(r => r.Status == ComplianceStatus.NonCompliant),
            NotAssessedCount = results.Count(r => r.Status == ComplianceStatus.NotAssessed),
            CriticalFindings = results.Count(r => r.RiskLevel == RiskLevel.Critical),
            HighRiskFindings = results.Count(r => r.RiskLevel == RiskLevel.High),
            MediumRiskFindings = results.Count(r => r.RiskLevel == RiskLevel.Medium),
            LowRiskFindings = results.Count(r => r.RiskLevel == RiskLevel.Low)
        };
    }
}
