using System.Text;
using IgnaCheck.Application.Common.Interfaces;
using Microsoft.Extensions.Logging;

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
            // TODO: Implement PDF parsing using PdfPig or iText7
            // For now, return a placeholder implementation
            _logger.LogWarning("PDF parsing not yet implemented. Returning placeholder result.");

            return await Task.FromResult(new DocumentParsingResult
            {
                ExtractedText = "[PDF parsing not yet implemented]",
                IsSuccessful = false,
                ErrorMessage = "PDF parsing support will be added in a future update.",
                ExtractionMethod = "Placeholder",
                PageCount = 0
            });

            /* Implementation example with PdfPig:
            using UglyToad.PdfPig;
            using UglyToad.PdfPig.Content;

            using var document = PdfDocument.Open(stream);
            var textBuilder = new StringBuilder();

            foreach (var page in document.GetPages())
            {
                textBuilder.AppendLine(page.Text);
            }

            return new DocumentParsingResult
            {
                ExtractedText = textBuilder.ToString(),
                IsSuccessful = true,
                ExtractionMethod = "PdfPig",
                PageCount = document.NumberOfPages,
                Title = document.Information?.Title,
                Author = document.Information?.Author,
                CreationDate = document.Information?.CreationDate
            };
            */
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
            // TODO: Implement DOCX parsing using DocumentFormat.OpenXml
            // For now, return a placeholder implementation
            _logger.LogWarning("DOCX parsing not yet implemented. Returning placeholder result.");

            return await Task.FromResult(new DocumentParsingResult
            {
                ExtractedText = "[DOCX parsing not yet implemented]",
                IsSuccessful = false,
                ErrorMessage = "DOCX parsing support will be added in a future update.",
                ExtractionMethod = "Placeholder"
            });

            /* Implementation example with DocumentFormat.OpenXml:
            using DocumentFormat.OpenXml.Packaging;
            using DocumentFormat.OpenXml.Wordprocessing;

            using var document = WordprocessingDocument.Open(stream, false);
            var body = document.MainDocumentPart?.Document.Body;

            if (body == null)
            {
                return new DocumentParsingResult
                {
                    ExtractedText = string.Empty,
                    IsSuccessful = false,
                    ErrorMessage = "Document body not found",
                    ExtractionMethod = "DocumentFormat.OpenXml"
                };
            }

            var textBuilder = new StringBuilder();
            foreach (var paragraph in body.Elements<Paragraph>())
            {
                textBuilder.AppendLine(paragraph.InnerText);
            }

            var coreProperties = document.PackageProperties;

            return new DocumentParsingResult
            {
                ExtractedText = textBuilder.ToString(),
                IsSuccessful = true,
                ExtractionMethod = "DocumentFormat.OpenXml",
                Title = coreProperties.Title,
                Author = coreProperties.Creator,
                CreationDate = coreProperties.Created
            };
            */
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
