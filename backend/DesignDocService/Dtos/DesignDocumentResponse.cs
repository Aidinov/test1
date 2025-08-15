using System.Text.Json.Serialization;
using DesignDocService.Models;

namespace DesignDocService.Dtos
{
    /// <summary>
    /// API response representing a design document. Content is included when the document
    /// is retrieved individually; for list queries the content may be omitted (null).
    /// </summary>
    public class DesignDocumentResponse
    {
        public Guid Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Product { get; set; } = string.Empty;
        public string Team { get; set; } = string.Empty;
        public string Author { get; set; } = string.Empty;
        public string TaskLink { get; set; } = string.Empty;
        /// <summary>
        /// Raw markdown content of the design document. This property is populated by the API
        /// when a single document is retrieved. It is not stored in the database.
        /// </summary>
        public string? Content { get; set; }
        public string GitRepository { get; set; } = string.Empty;
        public string GitFilePath { get; set; } = string.Empty;
        public string GitCommitHash { get; set; } = string.Empty;
        [JsonConverter(typeof(JsonStringEnumConverter))]
        public DocumentStatus Status { get; set; } = DocumentStatus.InProgress;
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public List<CommentResponse>? Comments { get; set; }
    }
}