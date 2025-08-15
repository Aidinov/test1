namespace DesignDocService.Models
{
    /// <summary>
    /// Represents a comment left on a specific range of text within a design document.
    /// </summary>
    public class Comment
    {
        /// <summary>
        /// Unique identifier of the comment.
        /// </summary>
        public Guid Id { get; set; } = Guid.NewGuid();

        /// <summary>
        /// Foreign key to the design document this comment belongs to.
        /// </summary>
        public Guid DocumentId { get; set; }

        /// <summary>
        /// Author of the comment.
        /// </summary>
        public string Author { get; set; } = string.Empty;

        /// <summary>
        /// The zero‑based index within the document content where the commented section starts.
        /// </summary>
        public int StartIndex { get; set; }

        /// <summary>
        /// The zero‑based index within the document content where the commented section ends (exclusive).
        /// </summary>
        public int EndIndex { get; set; }

        /// <summary>
        /// Type of the comment (question or remark).
        /// </summary>
        public CommentType Type { get; set; }

        /// <summary>
        /// Severity of the remark. Not relevant for questions.
        /// </summary>
        public RemarkSeverity Severity { get; set; } = RemarkSeverity.Opinion;

        /// <summary>
        /// Markdown content of the comment.
        /// </summary>
        public string Content { get; set; } = string.Empty;

        /// <summary>
        /// UTC time when the comment was created.
        /// </summary>
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// UTC time when the comment was last updated.
        /// </summary>
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// Indicates whether the comment has been resolved.
        /// </summary>
        public bool IsResolved { get; set; }

        /// <summary>
        /// Name of the person who resolved the comment.
        /// </summary>
        public string? ResolvedBy { get; set; }

        /// <summary>
        /// Time when the comment was resolved.
        /// </summary>
        public DateTime? ResolvedAt { get; set; }

        /// <summary>
        /// Identifier of the document version (Git commit hash) this comment refers to.
        /// This is used to map comments to specific versions of the file in the repository.
        /// </summary>
        public string? DocumentVersion { get; set; }

        /// <summary>
        /// The original snippet of text that this comment was attached to at the time of creation.
        /// Storing the original text allows the UI to display context for comments even if the
        /// underlying document has changed or the annotated section has been removed.  This
        /// property is persisted in the database to facilitate diff highlighting and handling of
        /// duplicate substrings within a document.
        /// </summary>
        public string? OriginalText { get; set; }
    }
}