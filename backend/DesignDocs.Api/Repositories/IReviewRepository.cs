namespace DesignDocs.Api.Repositories;

using DesignDocs.Api.Entities;

public interface IReviewRepository
{
    Task<bool> StartReviewAsync(Guid documentId, string baseCommit, IEnumerable<ReviewParticipant> participants);
    Task<object?> GetSummaryAsync(Guid documentId);
}
