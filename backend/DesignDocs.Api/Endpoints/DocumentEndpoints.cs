using DesignDocs.Api.Entities;
using DesignDocs.Api.Models;
using DesignDocs.Api.Repositories;
using DesignDocs.Api.Services;

namespace DesignDocs.Api.Endpoints;

public static class DocumentEndpoints
{
    public static void MapDocumentEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapGet("/api/docs", async (string? query, DocumentStatus? status, string? team, string? owner, string? tag, int page, int pageSize, IDocumentRepository repo) =>
        {
            var items = await repo.SearchAsync(query, status, team, owner, tag, page, pageSize);
            return Results.Ok(items);
        });

        app.MapGet("/api/docs/{id}", async (Guid id, IDocumentRepository repo) =>
        {
            return await repo.GetAsync(id) is { } doc ? Results.Ok(doc) : Results.NotFound();
        });

        app.MapPost("/api/docs", async (DocumentCreateRequest req, IDocumentRepository repo) =>
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
            await repo.AddAsync(doc);
            return Results.Created($"/api/docs/{doc.Id}", doc);
        });

        app.MapPut("/api/docs/{id}/content", async (Guid id, DocumentContentRequest req, IDocumentRepository repo) =>
        {
            var commit = Guid.NewGuid().ToString("N");
            var ok = await repo.UpdateCommitAsync(id, commit);
            return ok ? Results.NoContent() : Results.NotFound();
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
