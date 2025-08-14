namespace DesignDocs.Api.Repositories;

using DesignDocs.Api.Data;
using DesignDocs.Api.Entities;
using Microsoft.EntityFrameworkCore;

public class DocumentRepository : IDocumentRepository
{
    private readonly AppDbContext _db;
    public DocumentRepository(AppDbContext db) => _db = db;

    public async Task<IReadOnlyList<Document>> SearchAsync(string? query, DocumentStatus? status, string? team, string? owner, string? tag, int page, int pageSize)
    {
        var q = _db.Documents.AsQueryable();
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
        return await q.Skip((page - 1) * pageSize).Take(pageSize).ToListAsync();
    }

    public Task<Document?> GetAsync(Guid id) => _db.Documents.FindAsync(id).AsTask();

    public async Task AddAsync(Document document)
    {
        _db.Documents.Add(document);
        await _db.SaveChangesAsync();
    }

    public async Task<bool> UpdateCommitAsync(Guid id, string newCommit)
    {
        var doc = await _db.Documents.FindAsync(id);
        if (doc == null) return false;
        doc.CurrentCommit = newCommit;
        doc.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();
        return true;
    }
}
