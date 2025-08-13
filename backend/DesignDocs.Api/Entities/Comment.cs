namespace DesignDocs.Api.Entities;

public enum CommentType { Question, Issue, Suggestion }
public enum CommentSeverity { Blocker, Should, Nit }
public enum CommentStatus { Open, Resolved, WontFix }
public enum CommentResolution { Fixed, WontFix, Clarified }

public class Comment
{
    public Guid Id { get; set; }
    public Guid DocumentId { get; set; }
    public Document? Document { get; set; }
    public string AuthorId { get; set; } = string.Empty;
    public CommentType Type { get; set; }
    public CommentSeverity Severity { get; set; }
    public CommentStatus Status { get; set; } = CommentStatus.Open;
    public CommentResolution? Resolution { get; set; }
    public string? ResolvedBy { get; set; }
    public DateTime? ResolvedAt { get; set; }
    public string? ResolutionNote { get; set; }
    public string? ResolutionCommit { get; set; }
    public DateTime CreatedAt { get; set; }
    public ICollection<CommentAnchor> Anchors { get; set; } = new List<CommentAnchor>();
    public ICollection<CommentMessage> Messages { get; set; } = new List<CommentMessage>();
}
