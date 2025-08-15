using DesignDocService.Models;
using Microsoft.EntityFrameworkCore;

namespace DesignDocService.Services
{
    /// <summary>
    /// Provides CRUD operations backed by an EF Core DbContext for design documents and comments.
    /// </summary>
    public class DesignDocumentService(DesignDocContext context, IGitService gitService)
    {
        /// <summary>
        /// Returns all design documents including their comments.
        /// </summary>
        public async Task<List<DesignDocument>> GetAllAsync()
        {
            // When listing documents we deliberately do not load the content from the Git
            // repository to avoid unnecessary overhead.  The Content property is not
            // persisted in the database, so it will remain empty for documents returned here.
            return await context.DesignDocuments
                .Include(d => d.Comments)
                .ToListAsync();
        }

        /// <summary>
        /// Retrieves design documents filtered by optional criteria. Null or empty criteria are ignored.
        /// </summary>
        /// <param name="team">Team name to filter by.</param>
        /// <param name="product">Product name to filter by.</param>
        /// <param name="author">Author name to filter by.</param>
        public async Task<List<DesignDocument>> GetFilteredAsync(string? team, string? product, string? author)
        {
            var query = context.DesignDocuments.AsQueryable();
            if (!string.IsNullOrEmpty(team))
                query = query.Where(d => d.Team == team);
            if (!string.IsNullOrEmpty(product))
                query = query.Where(d => d.Product == product);
            if (!string.IsNullOrEmpty(author))
                query = query.Where(d => d.Author == author);
            return await query.Include(d => d.Comments).ToListAsync();
        }

        /// <summary>
        /// Retrieves a design document by its identifier.  Content is not loaded here; callers
        /// should use the Git service to fetch the markdown body when needed.
        /// </summary>
        public async Task<DesignDocument?> GetAsync(Guid id)
        {
            return await context.DesignDocuments
                .Include(d => d.Comments)
                .FirstOrDefaultAsync(d => d.Id == id);
        }

        /// <summary>
        /// Persists a new design document to the database.
        /// </summary>
        public async Task<DesignDocument> CreateAsync(DesignDocument doc, string content)
        {
            // Write the provided content to the Git store.  If the caller supplied a commit hash
            // we respect it; otherwise we assign the generated commit identifier from the Git
            // service.
            if (string.IsNullOrWhiteSpace(doc.GitCommitHash))
            {
                var commit = await gitService.WriteFileAsync(doc.GitRepository, doc.GitFilePath, content);
                doc.GitCommitHash = commit;
            }
            else
            {
                await gitService.WriteFileAsync(doc.GitRepository, doc.GitFilePath, content);
            }
            doc.Id = Guid.NewGuid();
            doc.CreatedAt = DateTime.UtcNow;
            doc.UpdatedAt = DateTime.UtcNow;
            context.DesignDocuments.Add(doc);
            await context.SaveChangesAsync();
            return doc;
        }

        /// <summary>
        /// Updates an existing document. Returns null if the document is not found.
        /// </summary>
        public async Task<DesignDocument?> UpdateAsync(Guid id, DesignDocument updated, string content)
        {
            var existing = await context.DesignDocuments
                .Include(d => d.Comments)
                .FirstOrDefaultAsync(d => d.Id == id);
            if (existing == null)
                return null;

            // Write updated content to the Git repository.  If the caller provided a
            // GitCommitHash we respect it; otherwise assign the generated commit from the Git service.
            var commitFromService = await gitService.WriteFileAsync(updated.GitRepository, updated.GitFilePath, content);
            if (string.IsNullOrWhiteSpace(updated.GitCommitHash))
            {
                existing.GitCommitHash = commitFromService;
            }
            else
            {
                existing.GitCommitHash = updated.GitCommitHash;
            }
            existing.Title = updated.Title;
            existing.Product = updated.Product;
            existing.Team = updated.Team;
            existing.Author = updated.Author;
            existing.TaskLink = updated.TaskLink;
            existing.GitRepository = updated.GitRepository;
            existing.GitFilePath = updated.GitFilePath;
            existing.Status = updated.Status;
            existing.UpdatedAt = DateTime.UtcNow;
            await context.SaveChangesAsync();
            return existing;
        }

        /// <summary>
        /// Updates the status of a document.
        /// </summary>
        public async Task<DesignDocument?> UpdateStatusAsync(Guid id, DocumentStatus status)
        {
            var existing = await context.DesignDocuments.FindAsync(id);
            if (existing == null) return null;
            existing.Status = status;
            existing.UpdatedAt = DateTime.UtcNow;
            await context.SaveChangesAsync();
            return existing;
        }

        /// <summary>
        /// Retrieves all comments for a document.
        /// </summary>
        public async Task<List<Comment>?> GetCommentsAsync(Guid documentId)
        {
            var document = await context.DesignDocuments
                .Include(d => d.Comments)
                .FirstOrDefaultAsync(d => d.Id == documentId);
            return document?.Comments;
        }

        /// <summary>
        /// Adds a comment to a document.
        /// </summary>
        public async Task<Comment?> AddCommentAsync(Guid documentId, Comment comment)
        {
            // Load the document including commit hash
            var doc = await context.DesignDocuments.FindAsync(documentId);
            if (doc == null)
                return null;

            // Validate indices relative to the current content of the document.  If the start
            // index exceeds the length of the file content we still allow the comment to be
            // created, but we cap the indices to the available range and preserve the
            // original text as empty string.  This behaviour ensures that even if the file
            // has been modified concurrently, the comment remains anchored to a version of
            // the document.
            var content = await gitService.ReadFileAsync(doc.GitRepository, doc.GitFilePath, doc.GitCommitHash);
            int start = comment.StartIndex;
            int end = comment.EndIndex;
            if (start < 0) start = 0;
            if (end < start) end = start;
            if (start > content.Length)
            {
                start = content.Length;
            }
            if (end > content.Length)
            {
                end = content.Length;
            }
            string snippet = string.Empty;
            if (end > start)
            {
                snippet = content.Substring(start, end - start);
            }

            comment.Id = Guid.NewGuid();
            comment.DocumentId = documentId;
            comment.CreatedAt = DateTime.UtcNow;
            comment.UpdatedAt = DateTime.UtcNow;
            // Tie comment to the version of the document at the time of comment creation
            comment.DocumentVersion = doc.GitCommitHash;
            comment.OriginalText = snippet;
            context.Comments.Add(comment);
            // Update document modification time
            var docToUpdate = await context.DesignDocuments.FindAsync(documentId);
            if (docToUpdate != null)
            {
                docToUpdate.UpdatedAt = DateTime.UtcNow;
            }
            await context.SaveChangesAsync();
            return comment;
        }

        /// <summary>
        /// Marks a comment as resolved.
        /// </summary>
        public async Task<Comment?> ResolveCommentAsync(Guid documentId, Guid commentId, string resolvedBy)
        {
            var comment = await context.Comments
                .FirstOrDefaultAsync(c => c.DocumentId == documentId && c.Id == commentId);
            if (comment == null) return null;
            if (!comment.IsResolved)
            {
                comment.IsResolved = true;
                comment.ResolvedBy = resolvedBy;
                comment.ResolvedAt = DateTime.UtcNow;
                comment.UpdatedAt = DateTime.UtcNow;
                var doc = await context.DesignDocuments.FindAsync(documentId);
                if (doc != null)
                {
                    doc.UpdatedAt = DateTime.UtcNow;
                }
                await context.SaveChangesAsync();
            }
            return comment;
        }
    }
}