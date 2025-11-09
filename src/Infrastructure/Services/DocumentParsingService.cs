using System.Text;
using IgnaCheck.Application.Common.Interfaces;
using Microsoft.Extensions.Logging;
using UglyToad.PdfPig;
using UglyToad.PdfPig.Content;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Wordprocessing;

namespace IgnaCheck.Infrastructure.Services;

/// <summary>
/// Service for parsing and extracting text from various document formats.
/// Uses: UglyToad.PdfPig for PDF, DocumentFormat.OpenXml for DOCX.
/// </summary>
public class DocumentParsingService : IDocumentParsingService
{
    private readonly ILogger<DocumentParsingService> _logger;

    public DocumentParsingService(ILogger<DocumentParsingService> logger)
    {
        _logger = logger;
    }

    public async Task<DocumentParsingResult> ParsePdfAsync(Stream stream, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Starting PDF parsing using PdfPig");

            // PdfPig is synchronous but we'll wrap in Task for consistency
            await Task.CompletedTask;

            using var document = PdfDocument.Open(stream);
            var textBuilder = new StringBuilder();

            // Extract text from each page
            foreach (var page in document.GetPages())
            {
                var pageText = page.Text;
                if (!string.IsNullOrWhiteSpace(pageText))
                {
                    textBuilder.AppendLine(pageText);
                    textBuilder.AppendLine(); // Add spacing between pages
                }
            }

            var extractedText = textBuilder.ToString().Trim();

            // Extract metadata
            var info = document.Information;
            var title = info?.Title;
            var author = info?.Author;
            DateTime? creationDate = null;

            // Parse creation date if available
            if (info?.CreationDate != null)
            {
                try
                {
                    creationDate = DateTime.Parse(info.CreationDate);
                }
                catch
                {
                    // If parsing fails, leave as null
                    _logger.LogWarning("Failed to parse PDF creation date: {CreationDate}", info.CreationDate);
                }
            }

            _logger.LogInformation("Successfully parsed PDF: {PageCount} pages, {TextLength} characters extracted",
                document.NumberOfPages, extractedText.Length);

            return new DocumentParsingResult
            {
                ExtractedText = extractedText,
                IsSuccessful = true,
                ExtractionMethod = "PdfPig",
                PageCount = document.NumberOfPages,
                Title = title,
                Author = author,
                CreationDate = creationDate
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error parsing PDF document");
            return new DocumentParsingResult
            {
                ExtractedText = string.Empty,
                IsSuccessful = false,
                ErrorMessage = $"Error parsing PDF: {ex.Message}",
                ExtractionMethod = "PdfPig"
            };
        }
    }

    public async Task<DocumentParsingResult> ParseDocxAsync(Stream stream, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Starting DOCX parsing using DocumentFormat.OpenXml");

            // OpenXml is synchronous but we'll wrap in Task for consistency
            await Task.CompletedTask;

            // Create a copy of the stream to avoid disposal issues
            using var memoryStream = new MemoryStream();
            await stream.CopyToAsync(memoryStream, cancellationToken);
            memoryStream.Position = 0;

            using var document = WordprocessingDocument.Open(memoryStream, false);
            var body = document.MainDocumentPart?.Document.Body;

            if (body == null)
            {
                _logger.LogWarning("DOCX document body not found");
                return new DocumentParsingResult
                {
                    ExtractedText = string.Empty,
                    IsSuccessful = false,
                    ErrorMessage = "Document body not found",
                    ExtractionMethod = "DocumentFormat.OpenXml"
                };
            }

            var textBuilder = new StringBuilder();

            // Extract text from paragraphs
            foreach (var paragraph in body.Elements<Paragraph>())
            {
                var paragraphText = paragraph.InnerText;
                if (!string.IsNullOrWhiteSpace(paragraphText))
                {
                    textBuilder.AppendLine(paragraphText);
                }
            }

            // Extract text from tables
            foreach (var table in body.Elements<Table>())
            {
                foreach (var row in table.Elements<TableRow>())
                {
                    var rowTexts = new List<string>();
                    foreach (var cell in row.Elements<TableCell>())
                    {
                        var cellText = cell.InnerText?.Trim();
                        if (!string.IsNullOrWhiteSpace(cellText))
                        {
                            rowTexts.Add(cellText);
                        }
                    }
                    if (rowTexts.Any())
                    {
                        textBuilder.AppendLine(string.Join(" | ", rowTexts));
                    }
                }
                textBuilder.AppendLine(); // Add spacing after table
            }

            var extractedText = textBuilder.ToString().Trim();

            // Extract metadata
            var coreProperties = document.PackageProperties;
            var title = coreProperties.Title;
            var author = coreProperties.Creator;
            var creationDate = coreProperties.Created;

            _logger.LogInformation("Successfully parsed DOCX: {TextLength} characters extracted",
                extractedText.Length);

            return new DocumentParsingResult
            {
                ExtractedText = extractedText,
                IsSuccessful = true,
                ExtractionMethod = "DocumentFormat.OpenXml",
                Title = title,
                Author = author,
                CreationDate = creationDate
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error parsing DOCX document");
            return new DocumentParsingResult
            {
                ExtractedText = string.Empty,
                IsSuccessful = false,
                ErrorMessage = $"Error parsing DOCX: {ex.Message}",
                ExtractionMethod = "DocumentFormat.OpenXml"
            };
        }
    }

    public async Task<DocumentParsingResult> ParseTextAsync(Stream stream, CancellationToken cancellationToken = default)
    {
        try
        {
            using var reader = new StreamReader(stream, Encoding.UTF8, detectEncodingFromByteOrderMarks: true, leaveOpen: true);
            var text = await reader.ReadToEndAsync(cancellationToken);

            return new DocumentParsingResult
            {
                ExtractedText = text,
                IsSuccessful = true,
                ExtractionMethod = "TextReader"
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error parsing text document");
            return new DocumentParsingResult
            {
                ExtractedText = string.Empty,
                IsSuccessful = false,
                ErrorMessage = $"Error parsing text file: {ex.Message}",
                ExtractionMethod = "TextReader"
            };
        }
    }

    public bool IsSupportedContentType(string contentType)
    {
        return contentType?.ToLowerInvariant() switch
        {
            "application/pdf" => true,
            "application/vnd.openxmlformats-officedocument.wordprocessingml.document" => true, // .docx
            "application/msword" => true, // .doc (legacy)
            "text/plain" => true,
            "text/markdown" => true,
            _ => false
        };
    }

    public async Task<DocumentParsingResult> ParseDocumentAsync(
        Stream stream,
        string contentType,
        CancellationToken cancellationToken = default)
    {
        if (!IsSupportedContentType(contentType))
        {
            return new DocumentParsingResult
            {
                ExtractedText = string.Empty,
                IsSuccessful = false,
                ErrorMessage = $"Unsupported content type: {contentType}",
                ExtractionMethod = "None"
            };
        }

        return contentType?.ToLowerInvariant() switch
        {
            "application/pdf" => await ParsePdfAsync(stream, cancellationToken),
            "application/vnd.openxmlformats-officedocument.wordprocessingml.document" => await ParseDocxAsync(stream, cancellationToken),
            "application/msword" => await ParseDocxAsync(stream, cancellationToken),
            "text/plain" or "text/markdown" => await ParseTextAsync(stream, cancellationToken),
            _ => new DocumentParsingResult
            {
                ExtractedText = string.Empty,
                IsSuccessful = false,
                ErrorMessage = $"No parser available for content type: {contentType}",
                ExtractionMethod = "None"
            }
        };
    }
}
