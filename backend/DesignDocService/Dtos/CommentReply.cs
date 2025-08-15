namespace DesignDocService.Dtos
{
    /// <summary>
    /// Represents a reply to a comment.
    /// </summary>
    public class CommentReply
    {
        public Guid Id { get; set; }
        public string Author { get; set; } = string.Empty;
        public string Content { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
    }
}
