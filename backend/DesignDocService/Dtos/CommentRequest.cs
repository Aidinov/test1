using System.Text.Json.Serialization;
using DesignDocService.Models;

namespace DesignDocService.Dtos
{
    /// <summary>
    /// Request payload used to create a new comment on a design document.
    /// </summary>
    public class CommentRequest
    {
        public int StartIndex { get; set; }
        public int EndIndex { get; set; }
        [JsonConverter(typeof(JsonStringEnumConverter))]
        public CommentType Type { get; set; }
        [JsonConverter(typeof(JsonStringEnumConverter))]
        public RemarkSeverity Severity { get; set; }
        public string Content { get; set; } = string.Empty;
        public string Author { get; set; } = string.Empty;
    }
}