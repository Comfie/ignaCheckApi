using System.Security.Cryptography;
using IgnaCheck.Application.Common.Interfaces;
using Microsoft.Extensions.Logging;

namespace IgnaCheck.Infrastructure.Services;

/// <summary>
/// Local file system storage implementation of IFileStorageService.
/// Stores files in a local directory structure: uploads/{organizationId}/{projectId}/{fileName}
/// For production, consider using Azure Blob Storage or AWS S3.
/// </summary>
public class LocalFileStorageService : IFileStorageService
{
    private readonly string _baseStoragePath;
    private readonly ILogger<LocalFileStorageService> _logger;

    public LocalFileStorageService(ILogger<LocalFileStorageService> logger)
    {
        _logger = logger;

        // Use a configurable base path (could be from configuration)
        // For now, use a path relative to the application
        _baseStoragePath = Path.Combine(Directory.GetCurrentDirectory(), "uploads");

        // Ensure base directory exists
        if (!Directory.Exists(_baseStoragePath))
        {
            Directory.CreateDirectory(_baseStoragePath);
            _logger.LogInformation("Created base storage directory at {Path}", _baseStoragePath);
        }
    }

    public async Task<string> UploadFileAsync(
        Stream stream,
        string fileName,
        string contentType,
        Guid organizationId,
        Guid projectId,
        CancellationToken cancellationToken = default)
    {
        try
        {
            // Create directory structure: uploads/{organizationId}/{projectId}
            var organizationDir = Path.Combine(_baseStoragePath, organizationId.ToString());
            var projectDir = Path.Combine(organizationDir, projectId.ToString());

            if (!Directory.Exists(projectDir))
            {
                Directory.CreateDirectory(projectDir);
            }

            // Generate unique filename to avoid collisions
            var fileExtension = Path.GetExtension(fileName);
            var uniqueFileName = $"{Guid.NewGuid()}{fileExtension}";
            var filePath = Path.Combine(projectDir, uniqueFileName);

            // Write file to disk
            using (var fileStream = new FileStream(filePath, FileMode.Create, FileAccess.Write, FileShare.None, 4096, useAsync: true))
            {
                await stream.CopyToAsync(fileStream, cancellationToken);
            }

            // Return relative path from base storage directory
            var storagePath = Path.Combine(organizationId.ToString(), projectId.ToString(), uniqueFileName);

            _logger.LogInformation("File uploaded successfully: {FileName} -> {StoragePath}", fileName, storagePath);

            return storagePath;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error uploading file {FileName} for organization {OrganizationId}, project {ProjectId}",
                fileName, organizationId, projectId);
            throw;
        }
    }

    public async Task<(Stream Stream, string ContentType)> DownloadFileAsync(
        string storagePath,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var fullPath = Path.Combine(_baseStoragePath, storagePath);

            if (!File.Exists(fullPath))
            {
                throw new FileNotFoundException($"File not found: {storagePath}");
            }

            // Read file into memory stream
            // For large files, consider returning FileStream directly
            var memoryStream = new MemoryStream();
            using (var fileStream = new FileStream(fullPath, FileMode.Open, FileAccess.Read, FileShare.Read, 4096, useAsync: true))
            {
                await fileStream.CopyToAsync(memoryStream, cancellationToken);
            }
            memoryStream.Position = 0;

            // Infer content type from extension
            var extension = Path.GetExtension(fullPath).ToLowerInvariant();
            var contentType = extension switch
            {
                ".pdf" => "application/pdf",
                ".docx" => "application/vnd.openxmlformats-officedocument.wordprocessingml.document",
                ".doc" => "application/msword",
                ".txt" => "text/plain",
                ".md" => "text/markdown",
                ".png" => "image/png",
                ".jpg" or ".jpeg" => "image/jpeg",
                ".gif" => "image/gif",
                _ => "application/octet-stream"
            };

            _logger.LogInformation("File downloaded successfully: {StoragePath}", storagePath);

            return (memoryStream, contentType);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error downloading file {StoragePath}", storagePath);
            throw;
        }
    }

    public Task DeleteFileAsync(string storagePath, CancellationToken cancellationToken = default)
    {
        try
        {
            var fullPath = Path.Combine(_baseStoragePath, storagePath);

            if (File.Exists(fullPath))
            {
                File.Delete(fullPath);
                _logger.LogInformation("File deleted successfully: {StoragePath}", storagePath);
            }
            else
            {
                _logger.LogWarning("File not found for deletion: {StoragePath}", storagePath);
            }

            return Task.CompletedTask;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting file {StoragePath}", storagePath);
            throw;
        }
    }

    public Task<bool> FileExistsAsync(string storagePath, CancellationToken cancellationToken = default)
    {
        var fullPath = Path.Combine(_baseStoragePath, storagePath);
        return Task.FromResult(File.Exists(fullPath));
    }

    public Task<long> GetFileSizeAsync(string storagePath, CancellationToken cancellationToken = default)
    {
        var fullPath = Path.Combine(_baseStoragePath, storagePath);

        if (!File.Exists(fullPath))
        {
            throw new FileNotFoundException($"File not found: {storagePath}");
        }

        var fileInfo = new FileInfo(fullPath);
        return Task.FromResult(fileInfo.Length);
    }

    public async Task<string> ComputeFileHashAsync(Stream stream, CancellationToken cancellationToken = default)
    {
        try
        {
            using var sha256 = SHA256.Create();
            var hashBytes = await sha256.ComputeHashAsync(stream, cancellationToken);

            // Reset stream position for subsequent reads
            if (stream.CanSeek)
            {
                stream.Position = 0;
            }

            // Convert to hexadecimal string
            return BitConverter.ToString(hashBytes).Replace("-", "").ToLowerInvariant();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error computing file hash");
            throw;
        }
    }
}
