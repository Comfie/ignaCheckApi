namespace IgnaCheck.Application.Common.Interfaces;

/// <summary>
/// Service for storing and retrieving files (documents, images, etc.).
/// Abstraction allows switching between local storage, Azure Blob Storage, AWS S3, etc.
/// </summary>
public interface IFileStorageService
{
    /// <summary>
    /// Uploads a file to storage.
    /// </summary>
    /// <param name="stream">File content stream</param>
    /// <param name="fileName">Original filename</param>
    /// <param name="contentType">MIME type</param>
    /// <param name="organizationId">Organization ID for multi-tenant isolation</param>
    /// <param name="projectId">Project ID for organization</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Storage path/key for retrieving the file</returns>
    Task<string> UploadFileAsync(
        Stream stream,
        string fileName,
        string contentType,
        Guid organizationId,
        Guid projectId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Downloads a file from storage.
    /// </summary>
    /// <param name="storagePath">Storage path returned from UploadFileAsync</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>File stream and content type</returns>
    Task<(Stream Stream, string ContentType)> DownloadFileAsync(
        string storagePath,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Deletes a file from storage.
    /// </summary>
    /// <param name="storagePath">Storage path returned from UploadFileAsync</param>
    /// <param name="cancellationToken">Cancellation token</param>
    Task DeleteFileAsync(
        string storagePath,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Checks if a file exists in storage.
    /// </summary>
    /// <param name="storagePath">Storage path to check</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>True if file exists, false otherwise</returns>
    Task<bool> FileExistsAsync(
        string storagePath,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets the file size in bytes.
    /// </summary>
    /// <param name="storagePath">Storage path</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>File size in bytes</returns>
    Task<long> GetFileSizeAsync(
        string storagePath,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Computes SHA256 hash of a file.
    /// </summary>
    /// <param name="stream">File content stream</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>SHA256 hash as hexadecimal string</returns>
    Task<string> ComputeFileHashAsync(
        Stream stream,
        CancellationToken cancellationToken = default);
}
