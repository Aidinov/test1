using DesignDocs.Api.Data;
using DesignDocs.Api.Endpoints;
using DesignDocs.Api.Entities;
using DesignDocs.Api.Services;
using Microsoft.EntityFrameworkCore;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

builder.Host.UseSerilog((ctx, lc) => lc
    .WriteTo.Console()
    .ReadFrom.Configuration(ctx.Configuration));

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var conn = Environment.GetEnvironmentVariable("DB_CONNECTION") ?? "Host=localhost;Database=design_docs;Username=postgres;Password=postgres";
builder.Services.AddDbContext<AppDbContext>(opt => opt.UseNpgsql(conn));

builder.Services.AddScoped<GitCliProvider>();
builder.Services.AddScoped<AnchorService>();
builder.Services.AddScoped<DiffService>();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// Documents
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

// Reviews
app.MapPost("/api/reviews", async (ReviewStartRequest req, AppDbContext db) =>
{
    var doc = await db.Documents.FindAsync(req.DocumentId);
    if (doc == null) return Results.NotFound();
    doc.BaseReviewCommit = req.BaseCommit;
    foreach (var p in req.Participants)
    {
        doc.Participants.Add(new ReviewParticipant
        {
            DocumentId = doc.Id,
            UserId = p.UserId,
            Role = p.Role
        });
    }
    await db.SaveChangesAsync();
    return Results.Ok();
});

app.MapGet("/api/reviews/{docId}/summary", async (Guid docId, AppDbContext db) =>
{
    var summary = new
    {
        Open = await db.Comments.CountAsync(c => c.DocumentId == docId && c.Status == CommentStatus.Open),
        Blockers = await db.Comments.CountAsync(c => c.DocumentId == docId && c.Severity == CommentSeverity.Blocker),
        NeedsRecheck = 0,
        Detached = await db.CommentAnchors.CountAsync(a => a.Comment!.DocumentId == docId && a.IsDetached)
    };
    return Results.Ok(summary);
});

// Comments
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

// Templates
var templatesDir = Path.Combine(Directory.GetCurrentDirectory(), "docs", "templates");

app.MapGet("/api/templates", () =>
{
    if (!Directory.Exists(templatesDir)) return Enumerable.Empty<string>();
    return Directory.GetFiles(templatesDir, "*.md").Select(f => Path.GetFileNameWithoutExtension(f));
});

app.MapGet("/api/templates/{name}", (string name) =>
{
    var path = Path.Combine(templatesDir, name + ".md");
    return File.Exists(path) ? Results.Text(File.ReadAllText(path), "text/markdown") : Results.NotFound();
});

// User
app.MapGet("/api/me", (HttpContext ctx) =>
{
    var user = ctx.Request.Headers["X-User"].FirstOrDefault() ?? "anonymous";
    return Results.Ok(new { Login = user });
});

app.MapGitlabWebhookEndpoints();

app.Run();

public record DocumentCreateRequest(string Template, string Title, string? Team, string? Owner, string? PathHints);
public record DocumentContentRequest(string Content);
public record ReviewStartRequest(Guid DocumentId, string BaseCommit, List<ReviewParticipantDto> Participants);
public record ReviewParticipantDto(string UserId, ParticipantRole Role);
public record CommentCreateRequest(Guid DocumentId, string AuthorId, CommentType Type, CommentSeverity Severity, CommentAnchorRequest Anchor);
public record CommentAnchorRequest(string BaseCommit, string BlockPath, int StartOffset, int EndOffset, string TextPrefix, string TextQuote, string TextSuffix, string BlockHash);
public record CommentMessageRequest(string AuthorId, string Body, Guid? ParentMessageId);
public record CommentResolveRequest(CommentResolution Resolution, string? Note, string? Commit);
public record CommentPatchRequest(CommentType? Type, CommentSeverity? Severity);

public partial class Program {}

