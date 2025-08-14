using DesignDocs.Api.Entities;
using DesignDocs.Api.Models;
using DesignDocs.Api.Repositories;

namespace DesignDocs.Api.Endpoints;

public static class CommentEndpoints
{
    public static void MapCommentEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapGet("/api/docs/{id}/comments", async (Guid id, CommentStatus? status, CommentType? type, CommentSeverity? severity, string? flags, ICommentRepository repo) =>
        {
            var detachedOnly = flags?.Split(',').Contains("detached") ?? false;
            var comments = await repo.GetForDocumentAsync(id, status, type, severity, detachedOnly);
            return Results.Ok(comments);
        });

        app.MapPost("/api/comments", async (CommentCreateRequest req, ICommentRepository repo) =>
        {
            var comment = new Comment
            {
                Id = Guid.NewGuid(),
                DocumentId = req.DocumentId,
                AuthorId = req.AuthorId,
                Type = req.Type,
                Severity = req.Severity,
                CreatedAt = DateTime.UtcNow
            };
            var anchor = new CommentAnchor
            {
                Id = Guid.NewGuid(),
                CommentId = comment.Id,
                BaseCommit = req.Anchor.BaseCommit,
                BlockPath = req.Anchor.BlockPath,
                StartOffset = req.Anchor.StartOffset,
                EndOffset = req.Anchor.EndOffset,
                TextPrefix = req.Anchor.TextPrefix,
                TextQuote = req.Anchor.TextQuote,
                TextSuffix = req.Anchor.TextSuffix,
                BlockHash = req.Anchor.BlockHash
            };
            comment.Anchors.Add(anchor);
            await repo.AddAsync(comment);
            return Results.Created($"/api/comments/{comment.Id}", comment);
        });

        app.MapPost("/api/comments/{id}/messages", async (Guid id, CommentMessageRequest req, ICommentRepository repo) =>
        {
            var msg = new CommentMessage
            {
                Id = Guid.NewGuid(),
                CommentId = id,
                AuthorId = req.AuthorId,
                Body = req.Body,
                ParentMessageId = req.ParentMessageId,
                CreatedAt = DateTime.UtcNow
            };
            await repo.AddMessageAsync(msg);
            return Results.Created($"/api/comments/{id}/messages/{msg.Id}", msg);
        });

        app.MapPost("/api/comments/{id}/resolve", async (Guid id, CommentResolveRequest req, ICommentRepository repo) =>
        {
            var comment = await repo.GetAsync(id);
            if (comment == null) return Results.NotFound();
            comment.Status = CommentStatus.Resolved;
            comment.Resolution = req.Resolution;
            comment.ResolutionNote = req.Note;
            comment.ResolutionCommit = req.Commit;
            comment.ResolvedAt = DateTime.UtcNow;
            await repo.UpdateAsync(comment);
            return Results.Ok(comment);
        });

        app.MapPost("/api/comments/{id}/reanchor", async (Guid id, CommentAnchorRequest req, ICommentRepository repo) =>
        {
            var anchor = await repo.GetAnchorAsync(id);
            if (anchor == null) return Results.NotFound();
            anchor.BaseCommit = req.BaseCommit;
            anchor.BlockPath = req.BlockPath;
            anchor.StartOffset = req.StartOffset;
            anchor.EndOffset = req.EndOffset;
            anchor.TextPrefix = req.TextPrefix;
            anchor.TextQuote = req.TextQuote;
            anchor.TextSuffix = req.TextSuffix;
            anchor.BlockHash = req.BlockHash;
            anchor.IsDetached = false;
            await repo.UpdateAnchorAsync(anchor);
            return Results.Ok();
        });

        app.MapPatch("/api/comments/{id}", async (Guid id, CommentPatchRequest req, ICommentRepository repo) =>
        {
            var comment = await repo.GetAsync(id);
            if (comment == null) return Results.NotFound();
            if (req.Type.HasValue) comment.Type = req.Type.Value;
            if (req.Severity.HasValue) comment.Severity = req.Severity.Value;
            await repo.UpdateAsync(comment);
            return Results.Ok(comment);
        });

        app.MapDelete("/api/comments/{id}", async (Guid id, ICommentRepository repo) =>
        {
            var comment = await repo.GetAsync(id);
            if (comment == null) return Results.NotFound();
            await repo.DeleteAsync(comment);
            return Results.NoContent();
        });
    }
}
