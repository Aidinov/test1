using DesignDocService.Dtos;
using DesignDocService.Mapping;
using DesignDocService.Models;
using DesignDocService.Services;

namespace DesignDocService.Endpoints
{
    /// <summary>
    /// Extension methods for registering document-related API endpoints.
    /// </summary>
    public static class DocumentEndpoints
    {
        /// <summary>
        /// Registers endpoints for creating, reading, updating and listing design documents.
        /// </summary>
        public static void MapDocumentEndpoints(this IEndpointRouteBuilder app)
        {
            var group = app.MapGroup("/api/documents");

            // List documents with optional filters and pagination
            group.MapGet("", async (DesignDocumentService service, [AsParameters] DesignDocumentQuery query) =>
            {
                var entities = await service.GetFilteredAsync(
                    query.Team,
                    query.Product,
                    query.Author,
                    query.Status,
                    query.Page ?? 1,
                    query.PageSize ?? 20);
                var summaries = entities.Select(e => e.ToSummary()).ToList();
                return Results.Ok(summaries);
            });

            // Get a document by id
            group.MapGet("/{id:guid}", async (DesignDocumentService service, IGitService gitService, Guid id) =>
            {
                var doc = await service.GetAsync(id);
                if (doc == null)
                    return Results.NotFound();
                // Load content from Git
                var content = await gitService.ReadFileAsync(doc.GitRepository, doc.GitFilePath, doc.GitCommitHash);
                var commentsDto = doc.Comments?.Select(c => c.ToResponseDto());
                var details = doc.ToDetails(content, commentsDto);
                return Results.Ok(details);
            });

            // Create a new document
            group.MapPost("", async (CreateDocumentRequest docDto, DesignDocumentService service) =>
            {
                // Validate required fields
                if (string.IsNullOrWhiteSpace(docDto.Title))
                    return Results.BadRequest("Title is required.");
                if (string.IsNullOrWhiteSpace(docDto.Product))
                    return Results.BadRequest("Product is required.");
                if (string.IsNullOrWhiteSpace(docDto.Team))
                    return Results.BadRequest("Team is required.");
                if (string.IsNullOrWhiteSpace(docDto.Author))
                    return Results.BadRequest("Author is required.");
                if (string.IsNullOrWhiteSpace(docDto.GitRepository) || string.IsNullOrWhiteSpace(docDto.GitFilePath))
                    return Results.BadRequest("Git repository and file path are required.");

                var entity = docDto.ToEntity();
                var created = await service.CreateAsync(entity, docDto.Content);
                var summary = created.ToSummary();
                return Results.Created($"/api/documents/{summary.Id}", summary);
            });

            // Update a document
            group.MapPut("/{id:guid}", async (Guid id, UpdateDocumentRequest updatedDto, DesignDocumentService service) =>
            {
                var updatedEntity = new DesignDocument
                {
                    Title = updatedDto.Title,
                    Product = updatedDto.Product,
                    Team = updatedDto.Team,
                    Author = updatedDto.Author,
                    TaskLink = updatedDto.TaskLink,
                    GitRepository = updatedDto.GitRepository,
                    GitFilePath = updatedDto.GitFilePath,
                    GitCommitHash = updatedDto.GitCommitHash ?? string.Empty,
                    Status = updatedDto.Status
                };
                var doc = await service.UpdateAsync(id, updatedEntity, updatedDto.Content);
                if (doc == null)
                    return Results.NotFound();
                var summary = doc.ToSummary();
                return Results.Ok(summary);
            });

            // Update document status only
            group.MapPut("/{id:guid}/status", async (Guid id, DocumentStatus status, DesignDocumentService service) =>
            {
                var doc = await service.UpdateStatusAsync(id, status);
                if (doc == null)
                    return Results.NotFound();
                var summary = doc.ToSummary();
                return Results.Ok(summary);
            });
        }
    }
}