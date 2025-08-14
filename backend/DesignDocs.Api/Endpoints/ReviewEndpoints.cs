using DesignDocs.Api.Entities;
using DesignDocs.Api.Models;
using DesignDocs.Api.Repositories;

namespace DesignDocs.Api.Endpoints;

public static class ReviewEndpoints
{
    public static void MapReviewEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapPost("/api/reviews", async (ReviewStartRequest req, IReviewRepository repo) =>
        {
            var participants = req.Participants.Select(p => new ReviewParticipant
            {
                DocumentId = req.DocumentId,
                UserId = p.UserId,
                Role = p.Role
            });
            var ok = await repo.StartReviewAsync(req.DocumentId, req.BaseCommit, participants);
            return ok ? Results.Ok() : Results.NotFound();
        });

        app.MapGet("/api/reviews/{docId}/summary", async (Guid docId, IReviewRepository repo) =>
        {
            var summary = await repo.GetSummaryAsync(docId);
            return summary == null ? Results.NotFound() : Results.Ok(summary);
        });
    }
}
