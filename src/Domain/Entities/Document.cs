namespace IgnaCheck.Domain.Entities;

/// <summary>
/// Represents a document uploaded to the system for compliance analysis.
/// Documents can be policies, procedures, logs, screenshots, or any evidence files.
/// </summary>
public class Document : BaseAuditableEntity, ITenantEntity
{
    /// <summary>
    /// Organization (tenant) that owns this document.
    /// </summary>
    public Guid OrganizationId { get; set; }

    /// <summary>
    /// The project this document belongs to.
    /// </summary>
    public Guid ProjectId { get; set; }

    /// <summary>
    /// Original filename.
    /// </summary>
    public string FileName { get; set; } = string.Empty;

    /// <summary>
    /// Display name/title for the document.
    /// </summary>
    public string DisplayName { get; set; } = string.Empty;

    /// <summary>
    /// Description or notes about the document.
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// MIME type (e.g., "application/pdf", "image/png").
    /// </summary>
    public string ContentType { get; set; } = string.Empty;

    /// <summary>
    /// File size in bytes.
    /// </summary>
    public long FileSizeBytes { get; set; }

    /// <summary>
    /// Storage path or blob key for retrieving the file.
    /// </summary>
    public string StoragePath { get; set; } = string.Empty;

    /// <summary>
    /// Hash of the file content (SHA256) for deduplication and integrity.
    /// </summary>
    public string? FileHash { get; set; }

    /// <summary>
    /// Category or type of document (e.g., "Policy", "Procedure", "Log", "Screenshot").
    /// </summary>
    public DocumentCategory Category { get; set; } = DocumentCategory.Other;

    /// <summary>
    /// Tags for categorization and search.
    /// Stored as JSON array.
    /// </summary>
    public string? Tags { get; set; }

    /// <summary>
    /// Extracted text content from the document (for search and AI analysis).
    /// Populated by OCR or PDF text extraction.
    /// </summary>
    public string? ExtractedText { get; set; }

    /// <summary>
    /// Extraction method used (e.g., "PDFSharp", "Tesseract OCR", "Manual").
    /// </summary>
    public string? ExtractionMethod { get; set; }

    /// <summary>
    /// Date when text was extracted.
    /// </summary>
    public DateTime? TextExtractedDate { get; set; }

    /// <summary>
    /// Indicates whether text extraction was successful.
    /// </summary>
    public bool IsTextExtracted { get; set; }

    /// <summary>
    /// Number of pages in the document (for PDFs).
    /// </summary>
    public int? PageCount { get; set; }

    /// <summary>
    /// Language of the document content (ISO 639-1 code, e.g., "en", "es").
    /// </summary>
    public string? Language { get; set; }

    /// <summary>
    /// Version number if this document has been updated.
    /// </summary>
    public int Version { get; set; } = 1;

    /// <summary>
    /// ID of the previous version if this is an updated document.
    /// </summary>
    public Guid? PreviousVersionId { get; set; }

    /// <summary>
    /// Indicates whether this is the latest version.
    /// </summary>
    public bool IsLatestVersion { get; set; } = true;

    /// <summary>
    /// Document author or creator.
    /// </summary>
    public string? Author { get; set; }

    /// <summary>
    /// Document creation date (metadata from file).
    /// </summary>
    public DateTime? DocumentDate { get; set; }

    /// <summary>
    /// Date when the document was uploaded to the system.
    /// </summary>
    public DateTime UploadedDate { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// User who uploaded the document.
    /// </summary>
    public string? UploadedBy { get; set; }

    /// <summary>
    /// Source URL if the document was imported from an external system.
    /// </summary>
    public string? SourceUrl { get; set; }

    /// <summary>
    /// External system identifier (e.g., SharePoint document ID, Google Drive file ID).
    /// </summary>
    public string? ExternalId { get; set; }

    /// <summary>
    /// Indicates whether this document is archived.
    /// </summary>
    public bool IsArchived { get; set; }

    /// <summary>
    /// Date when the document was archived.
    /// </summary>
    public DateTime? ArchivedDate { get; set; }

    /// <summary>
    /// Indicates whether AI analysis has been performed on this document.
    /// </summary>
    public bool IsAnalyzed { get; set; }

    /// <summary>
    /// Date of the last AI analysis.
    /// </summary>
    public DateTime? LastAnalyzedDate { get; set; }

    /// <summary>
    /// Vector embedding for semantic search.
    /// Stored as binary data (pgvector compatible).
    /// </summary>
    public string? EmbeddingVector { get; set; }

    /// <summary>
    /// Model used to generate the embedding (e.g., "text-embedding-ada-002").
    /// </summary>
    public string? EmbeddingModel { get; set; }

    // Navigation properties

    /// <summary>
    /// The organization this document belongs to.
    /// </summary>
    public Organization Organization { get; set; } = null!;

    /// <summary>
    /// The project this document belongs to.
    /// </summary>
    public Project Project { get; set; } = null!;

    /// <summary>
    /// Previous version of this document.
    /// </summary>
    public Document? PreviousVersion { get; set; }

    /// <summary>
    /// Newer versions of this document.
    /// </summary>
    public ICollection<Document> NewerVersions { get; set; } = new List<Document>();

    /// <summary>
    /// Finding evidence records that reference this document.
    /// </summary>
    public ICollection<FindingEvidence> FindingEvidences { get; set; } = new List<FindingEvidence>();

    /// <summary>
    /// Task attachments that reference this document.
    /// </summary>
    public ICollection<TaskAttachment> TaskAttachments { get; set; } = new List<TaskAttachment>();
}
