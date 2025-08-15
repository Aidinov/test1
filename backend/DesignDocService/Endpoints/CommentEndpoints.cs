using DesignDocService.Dtos;
using DesignDocService.Mapping;
using DesignDocService.Models;
using DesignDocService.Services;
using ModelComment = DesignDocService.Models.Comment;

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
                var responses = comments.Select(c => c.ToResponseDto()).ToList();
                return Results.Ok(responses);
            });

            // Add a new comment
            group.MapPost("", async (Guid documentId, CommentRequest commentDto, DesignDocumentService service) =>
            {
                if (commentDto.StartIndex < 0 || commentDto.EndIndex < 0 || commentDto.EndIndex <= commentDto.StartIndex)
                {
                    return Results.BadRequest("Invalid start/end indices.");
                }
                var comment = new ModelComment
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
                var response = created.ToResponseDto();
                return Results.Created($"/api/documents/{documentId}/comments/{response.Id}", response);
            });

            // Resolve an existing comment
            // Separate endpoint for resolving
            app.MapPost("/api/comments/{commentId:guid}/resolve", async (Guid commentId, ResolveRequest req, DesignDocumentService service) =>
            {
                var resolved = await service.ResolveCommentAsync(commentId, req.ResolvedBy);
                if (resolved == null)
                    return Results.NotFound();
                var response = resolved.ToResponseDto();
                return Results.Ok(response);
            });
        }
    }

    public record ResolveRequest(string ResolvedBy);
}