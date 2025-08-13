namespace DesignDocs.Api.Entities;

public class CommentAnchor
{
    public Guid Id { get; set; }
    public Guid CommentId { get; set; }
    public Comment? Comment { get; set; }
    public string BaseCommit { get; set; } = string.Empty;
    public string BlockPath { get; set; } = string.Empty;
    public int StartOffset { get; set; }
    public int EndOffset { get; set; }
    public string TextPrefix { get; set; } = string.Empty;
    public string TextQuote { get; set; } = string.Empty;
    public string TextSuffix { get; set; } = string.Empty;
    public string BlockHash { get; set; } = string.Empty;
    public bool IsDetached { get; set; }
    public string? LastCheckedCommit { get; set; }
}
