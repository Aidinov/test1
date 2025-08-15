using System.Text.Json.Serialization;
using DesignDocService.Models;

namespace DesignDocService.Dtos
{
    /// <summary>
    /// Detailed representation of a document including content and comments.
    /// </summary>
    public class DocumentDetails : DocumentSummary
    {
        /// <summary>
        /// Raw markdown content of the document.
        /// </summary>
        public string? Content { get; set; }

        /// <summary>
        /// Comments associated with the document.
        /// </summary>
        public List<CommentResponse>? Comments { get; set; }
    }
}
