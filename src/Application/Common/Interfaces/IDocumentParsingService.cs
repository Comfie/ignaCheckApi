namespace IgnaCheck.Application.Common.Interfaces;

/// <summary>
/// Service for parsing and extracting text from various document formats.
/// </summary>
public interface IDocumentParsingService
{
    /// <summary>
    /// Extracts text content from a PDF file.
    /// </summary>
    /// <param name="stream">PDF file stream</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Extracted text content and metadata</returns>
    Task<DocumentParsingResult> ParsePdfAsync(
        Stream stream,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Extracts text content from a DOCX file.
    /// </summary>
    /// <param name="stream">DOCX file stream</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Extracted text content and metadata</returns>
    Task<DocumentParsingResult> ParseDocxAsync(
        Stream stream,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Extracts text content from a plain text file.
    /// </summary>
    /// <param name="stream">Text file stream</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Extracted text content</returns>
    Task<DocumentParsingResult> ParseTextAsync(
        Stream stream,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Determines if a file type is supported for parsing.
    /// </summary>
    /// <param name="contentType">MIME type of the file</param>
    /// <returns>True if supported, false otherwise</returns>
    bool IsSupportedContentType(string contentType);

    /// <summary>
    /// Parses a document based on its content type.
    /// </summary>
    /// <param name="stream">Document stream</param>
    /// <param name="contentType">MIME type of the document</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Extracted text content and metadata</returns>
    Task<DocumentParsingResult> ParseDocumentAsync(
        Stream stream,
        string contentType,
        CancellationToken cancellationToken = default);
}

/// <summary>
/// Result of document parsing operation.
/// </summary>
public class DocumentParsingResult
{
    /// <summary>
    /// Extracted text content.
    /// </summary>
    public string ExtractedText { get; set; } = string.Empty;

    /// <summary>
    /// Indicates whether extraction was successful.
    /// </summary>
    public bool IsSuccessful { get; set; }

    /// <summary>
    /// Error message if extraction failed.
    /// </summary>
    public string? ErrorMessage { get; set; }

    /// <summary>
    /// Method used for extraction (e.g., "PDFSharp", "DocumentFormat.OpenXml", "TextReader").
    /// </summary>
    public string ExtractionMethod { get; set; } = string.Empty;

    /// <summary>
    /// Number of pages (for PDFs).
    /// </summary>
    public int? PageCount { get; set; }

    /// <summary>
    /// Document author (if available in metadata).
    /// </summary>
    public string? Author { get; set; }

    /// <summary>
    /// Document title (if available in metadata).
    /// </summary>
    public string? Title { get; set; }

    /// <summary>
    /// Document creation date (if available in metadata).
    /// </summary>
    public DateTime? CreationDate { get; set; }

    /// <summary>
    /// Detected language (if available).
    /// </summary>
    public string? Language { get; set; }
}
