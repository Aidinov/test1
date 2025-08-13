namespace DesignDocs.Api.Entities;

public class CommentMessage
{
    public Guid Id { get; set; }
    public Guid CommentId { get; set; }
    public Comment? Comment { get; set; }
    public string AuthorId { get; set; } = string.Empty;
    public string Body { get; set; } = string.Empty;
    public Guid? ParentMessageId { get; set; }
    public DateTime CreatedAt { get; set; }
}
