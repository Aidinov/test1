namespace DesignDocs.Api.Repositories;

using DesignDocs.Api.Data;
using DesignDocs.Api.Entities;
using Microsoft.EntityFrameworkCore;

public class CommentRepository : ICommentRepository
{
    private readonly AppDbContext _db;
    public CommentRepository(AppDbContext db) => _db = db;

    public async Task<IReadOnlyList<Comment>> GetForDocumentAsync(Guid documentId, CommentStatus? status, CommentType? type, CommentSeverity? severity, bool detachedOnly)
    {
        var q = _db.Comments.Include(c => c.Anchors).Where(c => c.DocumentId == documentId);
        if (status.HasValue) q = q.Where(c => c.Status == status);
        if (type.HasValue) q = q.Where(c => c.Type == type);
        if (severity.HasValue) q = q.Where(c => c.Severity == severity);
        if (detachedOnly) q = q.Where(c => c.Anchors.Any(a => a.IsDetached));
        return await q.ToListAsync();
    }

    public async Task AddAsync(Comment comment)
    {
        _db.Comments.Add(comment);
        await _db.SaveChangesAsync();
    }

    public async Task<CommentMessage> AddMessageAsync(CommentMessage message)
    {
        _db.CommentMessages.Add(message);
        await _db.SaveChangesAsync();
        return message;
    }

    public Task<Comment?> GetAsync(Guid id) => _db.Comments.FindAsync(id).AsTask();

    public Task<CommentAnchor?> GetAnchorAsync(Guid commentId) => _db.CommentAnchors.FirstOrDefaultAsync(a => a.CommentId == commentId);

    public async Task UpdateAsync(Comment comment)
    {
        _db.Comments.Update(comment);
        await _db.SaveChangesAsync();
    }

    public async Task UpdateAnchorAsync(CommentAnchor anchor)
    {
        _db.CommentAnchors.Update(anchor);
        await _db.SaveChangesAsync();
    }

    public async Task DeleteAsync(Comment comment)
    {
        _db.Comments.Remove(comment);
        await _db.SaveChangesAsync();
    }
}
