using DesignDocService.Dtos;
using DesignDocService.Models;
using DtoComment = DesignDocService.Dtos.Comment;
using ModelComment = DesignDocService.Models.Comment;

namespace DesignDocService.Mapping
{
    /// <summary>
    /// Provides extension methods for mapping between domain entities and DTOs.  These
    /// helpers centralize mapping logic and keep endpoint definitions concise.
    /// </summary>
    public static class MappingExtensions
    {
        /// <summary>
        /// Converts a <see cref="DesignDocument"/> entity to a <see cref="DocumentSummary"/>.
        /// </summary>
        public static DocumentSummary ToSummary(this DesignDocument entity)
        {
            return new DocumentSummary
            {
                Id = entity.Id,
                Title = entity.Title,
                Product = entity.Product,
                Team = entity.Team,
                Author = entity.Author,
                TaskLink = entity.TaskLink,
                GitRepository = entity.GitRepository,
                GitFilePath = entity.GitFilePath,
                GitCommitHash = entity.GitCommitHash,
                Status = entity.Status,
                CreatedAt = entity.CreatedAt,
                UpdatedAt = entity.UpdatedAt
            };
        }

        /// <summary>
        /// Converts a <see cref="DesignDocument"/> entity to a <see cref="DocumentDetails"/>.
        /// </summary>
        public static DocumentDetails ToDetails(this DesignDocument entity, string? content = null, IEnumerable<DtoComment>? comments = null)
        {
            return new DocumentDetails
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
        /// Converts a <see cref="ModelComment"/> entity to a <see cref="DtoComment"/> DTO.
        /// </summary>
        public static DtoComment ToDto(this ModelComment comment)
        {
            return new DtoComment
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
        /// Converts a collection of <see cref="Models.Comment"/> entities to DTOs.
        /// </summary>
        public static IEnumerable<DtoComment> ToDto(this IEnumerable<ModelComment> comments)
        {
            return comments.Select(c => c.ToDto());
        }

        /// <summary>
        /// Maps a create request DTO to a <see cref="DesignDocument"/> entity.  Note that
        /// content is handled separately and should be passed to the Git service when
        /// persisting the document.
        /// </summary>
        public static DesignDocument ToEntity(this CreateDocumentRequest request)
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
        /// </summary>
        public static void ApplyUpdates(this DesignDocument entity, UpdateDocumentRequest request)
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