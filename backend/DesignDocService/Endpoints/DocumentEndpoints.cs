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

            // List all documents or filter by optional query parameters
            group.MapGet("", async (DesignDocumentService service, [AsParameters] DesignDocumentQuery query) =>
            {
                List<DesignDocument> entities;
                if (!string.IsNullOrEmpty(query.Team) || !string.IsNullOrEmpty(query.Product) || !string.IsNullOrEmpty(query.Author))
                {
                    entities = await service.GetFilteredAsync(query.Team, query.Product, query.Author);
                }
                else
                {
                    entities = await service.GetAllAsync();
                }
                // For list responses, do not load content or comments.  Convert each entity to response with null content.
                var dtos = entities.Select(e => e.ToResponse(null, null)).ToList();
                return Results.Ok(dtos);
            });

            // Get a document by id
            group.MapGet("/{id:guid}", async (DesignDocumentService service, IGitService gitService, Guid id) =>
            {
                var doc = await service.GetAsync(id);
                if (doc == null)
                    return Results.NotFound();
                // Load content from Git
                var content = await gitService.ReadFileAsync(doc.GitRepository, doc.GitFilePath, doc.GitCommitHash);
                var commentsDto = doc.Comments?.Select(c => c.ToResponse());
                var response = doc.ToResponse(content, commentsDto);
                return Results.Ok(response);
            });

            // Create a new document
            group.MapPost("", async (CreateDesignDocumentRequest docDto, DesignDocumentService service) =>
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
                var response = created.ToResponse(null, null);
                return Results.Created($"/api/documents/{response.Id}", response);
            });

            // Update a document
            group.MapPut("/{id:guid}", async (Guid id, UpdateDesignDocumentRequest updatedDto, DesignDocumentService service) =>
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
                var response = doc.ToResponse(null, null);
                return Results.Ok(response);
            });

            // Update document status only
            group.MapPut("/{id:guid}/status", async (Guid id, DocumentStatus status, DesignDocumentService service) =>
            {
                var doc = await service.UpdateStatusAsync(id, status);
                if (doc == null)
                    return Results.NotFound();
                var response = doc.ToResponse(null, null);
                return Results.Ok(response);
            });
        }
    }
}