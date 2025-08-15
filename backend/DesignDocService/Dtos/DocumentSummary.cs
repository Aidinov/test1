using System.Text.Json.Serialization;
using DesignDocService.Models;

namespace DesignDocService.Dtos
{
    /// <summary>
    /// Lightweight representation of a document used for list views.
    /// </summary>
    public class DocumentSummary
    {
        public Guid Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Product { get; set; } = string.Empty;
        public string Team { get; set; } = string.Empty;
        public string Author { get; set; } = string.Empty;
        public string TaskLink { get; set; } = string.Empty;
        public string GitRepository { get; set; } = string.Empty;
        public string GitFilePath { get; set; } = string.Empty;
        public string GitCommitHash { get; set; } = string.Empty;
        [JsonConverter(typeof(JsonStringEnumConverter))]
        public DocumentStatus Status { get; set; } = DocumentStatus.Draft;
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }
}
