using IgnaCheck.Application.Common.Models;
using IgnaCheck.Application.Documents.Commands.DeleteDocument;
using IgnaCheck.Application.Documents.Commands.UploadDocument;
using IgnaCheck.Application.Documents.Queries.DownloadDocument;
using IgnaCheck.Application.Documents.Queries.GetDocumentsList;
using IgnaCheck.Domain.Enums;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace IgnaCheck.Web.Controllers;

/// <summary>
/// Controller for document management operations.
/// </summary>
[ApiController]
[Route("api/projects/{projectId}/[controller]")]
[Authorize]
public class DocumentsController : ApiControllerBase
{
    private readonly ISender _sender;

    public DocumentsController(ISender sender)
    {
        _sender = sender;
    }

    /// <summary>
    /// Get all documents in a project.
    /// </summary>
    /// <param name="projectId">Project ID</param>
    /// <param name="category">Filter by category (optional)</param>
    /// <param name="searchTerm">Search by name or description (optional)</param>
    /// <returns>List of documents</returns>
    [HttpGet]
    [ProducesResponseType(typeof(Result<List<DocumentDto>>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<Result<List<DocumentDto>>>> GetDocuments(
        Guid projectId,
        [FromQuery] DocumentCategory? category = null,
        [FromQuery] string? searchTerm = null)
    {
        var query = new GetDocumentsListQuery
        {
            ProjectId = projectId,
            Category = category,
            SearchTerm = searchTerm
        };

        var result = await _sender.Send(query);

        if (!result.Succeeded)
        {
            return BadRequest(result);
        }

        return Ok(result);
    }

    /// <summary>
    /// Upload a document to a project.
    /// </summary>
    /// <param name="projectId">Project ID</param>
    /// <param name="file">File to upload</param>
    /// <param name="category">Document category (optional)</param>
    /// <param name="description">Document description (optional)</param>
    /// <param name="tags">Tags (optional, comma-separated)</param>
    /// <returns>Uploaded document details</returns>
    [HttpPost]
    [RequestSizeLimit(26214400)] // 25 MB limit
    [ProducesResponseType(typeof(Result<UploadDocumentResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<Result<UploadDocumentResponse>>> UploadDocument(
        Guid projectId,
        IFormFile file,
        [FromForm] DocumentCategory? category = null,
        [FromForm] string? description = null,
        [FromForm] string? tags = null)
    {
        var command = new UploadDocumentCommand
        {
            ProjectId = projectId,
            File = file,
            Category = category,
            Description = description,
            Tags = tags
        };

        var result = await _sender.Send(command);

        if (!result.Succeeded)
        {
            return BadRequest(result);
        }

        return Ok(result);
    }

    /// <summary>
    /// Download a document.
    /// </summary>
    /// <param name="projectId">Project ID</param>
    /// <param name="id">Document ID</param>
    /// <returns>Document file</returns>
    [HttpGet("{id}/download")]
    [ProducesResponseType(typeof(FileStreamResult), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> DownloadDocument(Guid projectId, Guid id)
    {
        var query = new DownloadDocumentQuery { DocumentId = id };
        var result = await _sender.Send(query);

        if (!result.Succeeded)
        {
            return BadRequest(result);
        }

        return File(
            result.Data!.FileStream,
            result.Data.ContentType,
            result.Data.FileName,
            enableRangeProcessing: true
        );
    }

    /// <summary>
    /// Delete a document.
    /// </summary>
    /// <param name="projectId">Project ID</param>
    /// <param name="id">Document ID</param>
    /// <returns>Success result</returns>
    [HttpDelete("{id}")]
    [ProducesResponseType(typeof(Result), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<Result>> DeleteDocument(Guid projectId, Guid id)
    {
        var command = new DeleteDocumentCommand { DocumentId = id };
        var result = await _sender.Send(command);

        if (!result.Succeeded)
        {
            return BadRequest(result);
        }

        return Ok(result);
    }
}
