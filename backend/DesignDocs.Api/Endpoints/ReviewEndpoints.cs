using DesignDocs.Api.Data;
using DesignDocs.Api.Entities;
using Microsoft.EntityFrameworkCore;

namespace DesignDocs.Api.Endpoints;

public static class ReviewEndpoints
{
    public static void MapReviewEndpoints(this IEndpointRouteBuilder app)
    {
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
    }
}

public record ReviewStartRequest(Guid DocumentId, string BaseCommit, List<ReviewParticipantDto> Participants);
public record ReviewParticipantDto(string UserId, ParticipantRole Role);
