namespace DesignDocs.Api.Repositories;

using DesignDocs.Api.Data;
using DesignDocs.Api.Entities;
using Microsoft.EntityFrameworkCore;

public class ReviewRepository : IReviewRepository
{
    private readonly AppDbContext _db;
    public ReviewRepository(AppDbContext db) => _db = db;

    public async Task<bool> StartReviewAsync(Guid documentId, string baseCommit, IEnumerable<ReviewParticipant> participants)
    {
        var doc = await _db.Documents.Include(d => d.Participants).FirstOrDefaultAsync(d => d.Id == documentId);
        if (doc == null) return false;
        doc.BaseReviewCommit = baseCommit;
        foreach (var p in participants)
        {
            doc.Participants.Add(p);
        }
        await _db.SaveChangesAsync();
        return true;
    }

    public async Task<object?> GetSummaryAsync(Guid documentId)
    {
        var docExists = await _db.Documents.AnyAsync(d => d.Id == documentId);
        if (!docExists) return null;
        var summary = new
        {
            Open = await _db.Comments.CountAsync(c => c.DocumentId == documentId && c.Status == CommentStatus.Open),
            Blockers = await _db.Comments.CountAsync(c => c.DocumentId == documentId && c.Severity == CommentSeverity.Blocker),
            NeedsRecheck = 0,
            Detached = await _db.CommentAnchors.CountAsync(a => a.Comment!.DocumentId == documentId && a.IsDetached)
        };
        return summary;
    }
}
