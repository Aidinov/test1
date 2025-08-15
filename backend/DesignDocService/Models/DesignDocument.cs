namespace DesignDocService.Models
{
    /// <summary>
    /// Represents a design document containing technical decisions and context.
    /// </summary>
    public class DesignDocument
    {
        /// <summary>
        /// Unique identifier of the document.
        /// </summary>
        public Guid Id { get; set; } = Guid.NewGuid();

        /// <summary>
        /// Title of the design document.
        /// </summary>
        public string Title { get; set; } = string.Empty;

        /// <summary>
        /// Identifier or name of the product this document belongs to.
        /// </summary>
        public string Product { get; set; } = string.Empty;

        /// <summary>
        /// Identifier or name of the team authoring the document.
        /// </summary>
        public string Team { get; set; } = string.Empty;

        /// <summary>
        /// Name or identifier of the author of the document.
        /// </summary>
        public string Author { get; set; } = string.Empty;

        /// <summary>
        /// Optional link to the task in an external tracker.
        /// </summary>
        public string TaskLink { get; set; } = string.Empty;


        /// <summary>
        /// URL or identifier of the Git repository where the document is stored.
        /// </summary>
        public string GitRepository { get; set; } = string.Empty;

        /// <summary>
        /// Path to the file within the Git repository that contains the markdown document.
        /// </summary>
        public string GitFilePath { get; set; } = string.Empty;

        /// <summary>
        /// Commit hash or version identifier associated with the latest update of the design document.
        /// This allows comments to be tied to a specific version of the file in Git.
        /// </summary>
        public string GitCommitHash { get; set; } = string.Empty;

        /// <summary>
        /// Current status of the document.
        /// </summary>
        public DocumentStatus Status { get; set; } = DocumentStatus.InProgress;

        /// <summary>
        /// Creation timestamp in UTC.
        /// </summary>
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// Last modification timestamp in UTC.
        /// </summary>
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// Collection of comments attached to this document.
        /// </summary>
        public List<Comment> Comments { get; set; } = new();
    }
}