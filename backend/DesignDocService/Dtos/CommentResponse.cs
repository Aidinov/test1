using System.Text.Json.Serialization;
using DesignDocService.Models;

namespace DesignDocService.Dtos
{
    /// <summary>
    /// API response representing a comment. Includes original text snippet for context and
    /// document version (commit hash) to which the comment refers.
    /// </summary>
    public class CommentResponse
    {
        public Guid Id { get; set; }
        public Guid DocumentId { get; set; }
        public string Author { get; set; } = string.Empty;
        public int StartIndex { get; set; }
        public int EndIndex { get; set; }
        [JsonConverter(typeof(JsonStringEnumConverter))]
        public CommentType Type { get; set; }
        [JsonConverter(typeof(JsonStringEnumConverter))]
        public RemarkSeverity Severity { get; set; }
        public string Content { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public bool IsResolved { get; set; }
        public string? ResolvedBy { get; set; }
        public DateTime? ResolvedAt { get; set; }
        public string? DocumentVersion { get; set; }
        public string? OriginalText { get; set; }
    }
}