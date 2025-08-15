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
        public CommentType Type { get; set; }
        public RemarkSeverity Severity { get; set; }
        public string Content { get; set; } = string.Empty;
        public string Author { get; set; } = string.Empty;
    }
}