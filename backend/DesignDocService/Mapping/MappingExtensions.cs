using DesignDocService.Dtos;
using DesignDocService.Models;

namespace DesignDocService.Mapping
{
    /// <summary>
    /// Provides extension methods for mapping between domain entities and DTOs.  These
    /// helpers centralize mapping logic and keep endpoint definitions concise.
    /// </summary>
    public static class MappingExtensions
    {
        /// <summary>
        /// Converts a <see cref="DesignDocument"/> entity to a <see cref="DesignDocumentResponse"/>.
        /// Optionally accepts preloaded content and comment DTOs.  When content is null the
        /// caller should load it via the Git service.
        /// </summary>
        public static DesignDocumentResponse ToResponse(this DesignDocument entity, string? content = null, IEnumerable<CommentResponse>? comments = null)
        {
            return new DesignDocumentResponse
            {
                Id = entity.Id,
                Title = entity.Title,
                Product = entity.Product,
                Team = entity.Team,
                Author = entity.Author,
                TaskLink = entity.TaskLink,
                Content = content,
                GitRepository = entity.GitRepository,
                GitFilePath = entity.GitFilePath,
                GitCommitHash = entity.GitCommitHash,
                Status = entity.Status,
                CreatedAt = entity.CreatedAt,
                UpdatedAt = entity.UpdatedAt,
                Comments = comments?.ToList()
            };
        }

        /// <summary>
        /// Converts a <see cref="Comment"/> entity to a <see cref="CommentResponse"/> DTO.
        /// </summary>
        public static CommentResponse ToResponse(this Comment comment)
        {
            return new CommentResponse
            {
                Id = comment.Id,
                DocumentId = comment.DocumentId,
                Author = comment.Author,
                StartIndex = comment.StartIndex,
                EndIndex = comment.EndIndex,
                Type = comment.Type,
                Severity = comment.Severity,
                Content = comment.Content,
                CreatedAt = comment.CreatedAt,
                UpdatedAt = comment.UpdatedAt,
                IsResolved = comment.IsResolved,
                ResolvedBy = comment.ResolvedBy,
                ResolvedAt = comment.ResolvedAt,
                DocumentVersion = comment.DocumentVersion,
                OriginalText = comment.OriginalText
            };
        }

        /// <summary>
        /// Converts a collection of <see cref="Comment"/> entities to response DTOs.
        /// </summary>
        public static IEnumerable<CommentResponse> ToResponse(this IEnumerable<Comment> comments)
        {
            return comments.Select(c => c.ToResponse());
        }

        /// <summary>
        /// Maps a create request DTO to a <see cref="DesignDocument"/> entity.  Note that
        /// content is handled separately and should be passed to the Git service when
        /// persisting the document.
        /// </summary>
        public static DesignDocument ToEntity(this CreateDesignDocumentRequest request)
        {
            return new DesignDocument
            {
                Title = request.Title,
                Product = request.Product,
                Team = request.Team,
                Author = request.Author,
                TaskLink = request.TaskLink,
                GitRepository = request.GitRepository,
                GitFilePath = request.GitFilePath,
                GitCommitHash = request.GitCommitHash ?? string.Empty,
                Status = request.Status,
                CreatedAt = System.DateTime.UtcNow,
                UpdatedAt = System.DateTime.UtcNow
            };
        }

        /// <summary>
        /// Maps an update request DTO to an existing <see cref="DesignDocument"/> entity.
        /// Only properties that are present in the request will overwrite existing values.
        /// </summary>
        public static void ApplyUpdates(this DesignDocument entity, UpdateDesignDocumentRequest request)
        {
            entity.Title = request.Title;
            entity.Product = request.Product;
            entity.Team = request.Team;
            entity.Author = request.Author;
            entity.TaskLink = request.TaskLink;
            entity.GitRepository = request.GitRepository;
            entity.GitFilePath = request.GitFilePath;
            if (!string.IsNullOrWhiteSpace(request.GitCommitHash))
            {
                entity.GitCommitHash = request.GitCommitHash;
            }
            entity.Status = request.Status;
            entity.UpdatedAt = System.DateTime.UtcNow;
        }
    }
}