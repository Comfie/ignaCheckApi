namespace IgnaCheck.Infrastructure.AI;

/// <summary>
/// Configuration for AI services (Claude and ChatGPT).
/// </summary>
public class AIConfiguration
{
    public const string SectionName = "AI";

    /// <summary>
    /// Primary AI provider to use (Claude or OpenAI).
    /// </summary>
    public AIProvider Provider { get; set; } = AIProvider.Claude;

    /// <summary>
    /// Anthropic Claude settings.
    /// </summary>
    public ClaudeSettings Claude { get; set; } = new();

    /// <summary>
    /// OpenAI ChatGPT settings.
    /// </summary>
    public OpenAISettings OpenAI { get; set; } = new();

    /// <summary>
    /// Fallback to secondary provider if primary fails.
    /// </summary>
    public bool EnableFallback { get; set; } = true;

    /// <summary>
    /// Maximum tokens for AI responses.
    /// </summary>
    public int MaxTokens { get; set; } = 4096;

    /// <summary>
    /// Temperature for AI responses (0.0-1.0). Lower = more focused, Higher = more creative.
    /// </summary>
    public double Temperature { get; set; } = 0.3;

    /// <summary>
    /// Request timeout in seconds.
    /// </summary>
    public int TimeoutSeconds { get; set; } = 120;

    /// <summary>
    /// Enable caching of extracted text (to avoid re-extraction).
    /// </summary>
    public bool CacheExtractedText { get; set; } = false;

    /// <summary>
    /// Cache duration in hours (if caching is enabled).
    /// </summary>
    public int CacheDurationHours { get; set; } = 24;
}

/// <summary>
/// Claude (Anthropic) specific settings.
/// </summary>
public class ClaudeSettings
{
    /// <summary>
    /// Anthropic API key.
    /// </summary>
    public string ApiKey { get; set; } = string.Empty;

    /// <summary>
    /// Model to use (claude-3-5-sonnet-20241022, claude-3-opus-20240229, etc.).
    /// </summary>
    public string Model { get; set; } = "claude-3-5-sonnet-20241022";

    /// <summary>
    /// API endpoint URL (optional, defaults to Anthropic's API).
    /// </summary>
    public string? ApiEndpoint { get; set; }
}

/// <summary>
/// OpenAI (ChatGPT) specific settings.
/// </summary>
public class OpenAISettings
{
    /// <summary>
    /// OpenAI API key.
    /// </summary>
    public string ApiKey { get; set; } = string.Empty;

    /// <summary>
    /// Model to use (gpt-4o, gpt-4-turbo, etc.).
    /// </summary>
    public string Model { get; set; } = "gpt-4o";

    /// <summary>
    /// Organization ID (optional).
    /// </summary>
    public string? OrganizationId { get; set; }

    /// <summary>
    /// Azure OpenAI endpoint (if using Azure instead of OpenAI).
    /// </summary>
    public string? AzureEndpoint { get; set; }

    /// <summary>
    /// Use Azure OpenAI instead of OpenAI API.
    /// </summary>
    public bool UseAzure { get; set; } = false;
}

/// <summary>
/// AI provider enumeration.
/// </summary>
public enum AIProvider
{
    /// <summary>
    /// Anthropic Claude (default, recommended for compliance analysis).
    /// </summary>
    Claude = 0,

    /// <summary>
    /// OpenAI ChatGPT.
    /// </summary>
    OpenAI = 1
}
