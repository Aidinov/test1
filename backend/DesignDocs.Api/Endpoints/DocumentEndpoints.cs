using DesignDocs.Api.Data;
using DesignDocs.Api.Entities;
using DesignDocs.Api.Services;
using Microsoft.EntityFrameworkCore;

namespace DesignDocs.Api.Endpoints;

public static class DocumentEndpoints
{
    public static void MapDocumentEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapGet("/api/docs", async (string? query, DocumentStatus? status, string? team, string? owner, string? tag, int page, int pageSize, AppDbContext db) =>
        {
            var q = db.Documents.AsQueryable();
            if (!string.IsNullOrWhiteSpace(query))
                q = q.Where(d => d.Title.Contains(query));
            if (status.HasValue)
                q = q.Where(d => d.Status == status);
            if (!string.IsNullOrWhiteSpace(team))
                q = q.Where(d => d.Team == team);
            if (!string.IsNullOrWhiteSpace(owner))
                q = q.Where(d => d.Owner == owner);
            if (!string.IsNullOrWhiteSpace(tag))
                q = q.Where(d => d.Tags.Contains(tag));
            page = page <= 0 ? 1 : page;
            pageSize = pageSize <= 0 ? 20 : pageSize;
            var items = await q.Skip((page - 1) * pageSize).Take(pageSize).ToListAsync();
            return Results.Ok(items);
        });

        app.MapGet("/api/docs/{id}", async (Guid id, AppDbContext db) =>
        {
            return await db.Documents.FindAsync(id) is { } doc ? Results.Ok(doc) : Results.NotFound();
        });

        app.MapPost("/api/docs", async (DocumentCreateRequest req, AppDbContext db) =>
        {
            var slug = req.Title.ToLower().Replace(' ', '-');
            var doc = new Document
            {
                Id = Guid.NewGuid(),
                Title = req.Title,
                Slug = slug,
                Team = req.Team,
                Owner = req.Owner,
                RepoPath = req.PathHints ?? slug,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };
            db.Documents.Add(doc);
            await db.SaveChangesAsync();
            return Results.Created($"/api/docs/{doc.Id}", doc);
        });

        app.MapPut("/api/docs/{id}/content", async (Guid id, DocumentContentRequest req, AppDbContext db) =>
        {
            var doc = await db.Documents.FindAsync(id);
            if (doc == null) return Results.NotFound();
            doc.CurrentCommit = Guid.NewGuid().ToString("N");
            doc.UpdatedAt = DateTime.UtcNow;
            await db.SaveChangesAsync();
            return Results.NoContent();
        });

        app.MapGet("/api/docs/{id}/content", (Guid id, string? commit) =>
        {
            var content = $"# Document {id}\nCommit: {commit ?? "head"}";
            return Results.Text(content, "text/markdown");
        });

        app.MapGet("/api/docs/{id}/diff", (Guid id, string from, string to, DiffService diffSvc) =>
        {
            var model = diffSvc.Diff("old", "new");
            return Results.Ok(model);
        });
    }
}

public record DocumentCreateRequest(string Template, string Title, string? Team, string? Owner, string? PathHints);
public record DocumentContentRequest(string Content);
