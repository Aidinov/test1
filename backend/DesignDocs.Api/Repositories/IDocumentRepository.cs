namespace DesignDocs.Api.Repositories;

using DesignDocs.Api.Entities;

public interface IDocumentRepository
{
    Task<IReadOnlyList<Document>> SearchAsync(string? query, DocumentStatus? status, string? team, string? owner, string? tag, int page, int pageSize);
    Task<Document?> GetAsync(Guid id);
    Task AddAsync(Document document);
    Task<bool> UpdateCommitAsync(Guid id, string newCommit);
}
