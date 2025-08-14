using DesignDocs.Api.Data;
using DesignDocs.Api.Entities;
using Microsoft.EntityFrameworkCore;

namespace DesignDocs.Api.Endpoints;

public static class CommentEndpoints
{
    public static void MapCommentEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapGet("/api/docs/{id}/comments", async (Guid id, CommentStatus? status, CommentType? type, CommentSeverity? severity, string? flags, AppDbContext db) =>
        {
            var q = db.Comments.Include(c => c.Anchors).Where(c => c.DocumentId == id);
            if (status.HasValue) q = q.Where(c => c.Status == status);
            if (type.HasValue) q = q.Where(c => c.Type == type);
            if (severity.HasValue) q = q.Where(c => c.Severity == severity);
            if (!string.IsNullOrEmpty(flags))
            {
                var parts = flags.Split(',');
                if (parts.Contains("detached"))
                    q = q.Where(c => c.Anchors.Any(a => a.IsDetached));
            }
            return Results.Ok(await q.ToListAsync());
        });

        app.MapPost("/api/comments", async (CommentCreateRequest req, AppDbContext db) =>
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
            db.Comments.Add(comment);
            await db.SaveChangesAsync();
            return Results.Created($"/api/comments/{comment.Id}", comment);
        });

        app.MapPost("/api/comments/{id}/messages", async (Guid id, CommentMessageRequest req, AppDbContext db) =>
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
            db.CommentMessages.Add(msg);
            await db.SaveChangesAsync();
            return Results.Created($"/api/comments/{id}/messages/{msg.Id}", msg);
        });

        app.MapPost("/api/comments/{id}/resolve", async (Guid id, CommentResolveRequest req, AppDbContext db) =>
        {
            var comment = await db.Comments.FindAsync(id);
            if (comment == null) return Results.NotFound();
            comment.Status = CommentStatus.Resolved;
            comment.Resolution = req.Resolution;
            comment.ResolutionNote = req.Note;
            comment.ResolutionCommit = req.Commit;
            comment.ResolvedAt = DateTime.UtcNow;
            await db.SaveChangesAsync();
            return Results.Ok(comment);
        });

        app.MapPost("/api/comments/{id}/reanchor", async (Guid id, CommentAnchorRequest req, AppDbContext db) =>
        {
            var anchor = await db.CommentAnchors.FirstOrDefaultAsync(a => a.CommentId == id);
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
            await db.SaveChangesAsync();
            return Results.Ok();
        });

        app.MapPatch("/api/comments/{id}", async (Guid id, CommentPatchRequest req, AppDbContext db) =>
        {
            var comment = await db.Comments.FindAsync(id);
            if (comment == null) return Results.NotFound();
            if (req.Type.HasValue) comment.Type = req.Type.Value;
            if (req.Severity.HasValue) comment.Severity = req.Severity.Value;
            await db.SaveChangesAsync();
            return Results.Ok(comment);
        });

        app.MapDelete("/api/comments/{id}", async (Guid id, AppDbContext db) =>
        {
            var comment = await db.Comments.FindAsync(id);
            if (comment == null) return Results.NotFound();
            db.Comments.Remove(comment);
            await db.SaveChangesAsync();
            return Results.NoContent();
        });
    }
}

public record CommentCreateRequest(Guid DocumentId, string AuthorId, CommentType Type, CommentSeverity Severity, CommentAnchorRequest Anchor);
public record CommentAnchorRequest(string BaseCommit, string BlockPath, int StartOffset, int EndOffset, string TextPrefix, string TextQuote, string TextSuffix, string BlockHash);
public record CommentMessageRequest(string AuthorId, string Body, Guid? ParentMessageId);
public record CommentResolveRequest(CommentResolution Resolution, string? Note, string? Commit);
public record CommentPatchRequest(CommentType? Type, CommentSeverity? Severity);
