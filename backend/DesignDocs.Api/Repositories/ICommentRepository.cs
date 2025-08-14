namespace DesignDocs.Api.Repositories;

using DesignDocs.Api.Entities;

public interface ICommentRepository
{
    Task<IReadOnlyList<Comment>> GetForDocumentAsync(Guid documentId, CommentStatus? status, CommentType? type, CommentSeverity? severity, bool detachedOnly);
    Task AddAsync(Comment comment);
    Task<CommentMessage> AddMessageAsync(CommentMessage message);
    Task<Comment?> GetAsync(Guid id);
    Task<CommentAnchor?> GetAnchorAsync(Guid commentId);
    Task UpdateAsync(Comment comment);
    Task UpdateAnchorAsync(CommentAnchor anchor);
    Task DeleteAsync(Comment comment);
}
