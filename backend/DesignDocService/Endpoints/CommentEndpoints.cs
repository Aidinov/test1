using DesignDocService.Dtos;
using DesignDocService.Mapping;
using DesignDocService.Models;
using DesignDocService.Services;

namespace DesignDocService.Endpoints
{
    /// <summary>
    /// Extension methods for registering comment-related API endpoints.
    /// </summary>
    public static class CommentEndpoints
    {
        /// <summary>
        /// Registers endpoints for listing, adding and resolving comments on documents.
        /// </summary>
        public static void MapCommentEndpoints(this IEndpointRouteBuilder app)
        {
            // Comments are nested under documents, so we use a nested group
            var group = app.MapGroup("/api/documents/{documentId:guid}/comments");

            // List comments for a document
            group.MapGet("", async (Guid documentId, DesignDocumentService service) =>
            {
                var comments = await service.GetCommentsAsync(documentId);
                if (comments == null)
                    return Results.NotFound();
                var responses = comments.Select(c => c.ToResponse()).ToList();
                return Results.Ok(responses);
            });

            // Add a new comment
            group.MapPost("", async (Guid documentId, CommentRequest commentDto, DesignDocumentService service) =>
            {
                if (commentDto.StartIndex < 0 || commentDto.EndIndex < 0 || commentDto.EndIndex <= commentDto.StartIndex)
                {
                    return Results.BadRequest("Invalid start/end indices.");
                }
                var comment = new Comment
                {
                    Author = commentDto.Author,
                    StartIndex = commentDto.StartIndex,
                    EndIndex = commentDto.EndIndex,
                    Type = commentDto.Type,
                    Severity = commentDto.Severity,
                    Content = commentDto.Content,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow,
                    IsResolved = false
                };
                var created = await service.AddCommentAsync(documentId, comment);
                if (created == null)
                    return Results.NotFound();
                var response = created.ToResponse();
                return Results.Created($"/api/documents/{documentId}/comments/{response.Id}", response);
            });

            // Resolve an existing comment
            group.MapPost("/{commentId:guid}/resolve", async (Guid documentId, Guid commentId, string resolvedBy, DesignDocumentService service) =>
            {
                var resolved = await service.ResolveCommentAsync(documentId, commentId, resolvedBy);
                if (resolved == null)
                    return Results.NotFound();
                var response = resolved.ToResponse();
                return Results.Ok(response);
            });
        }
    }
}