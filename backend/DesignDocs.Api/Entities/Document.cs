namespace DesignDocs.Api.Entities;

public enum DocumentStatus { Draft, InReview, Approved, Archived }
public enum DocumentScope { SingleTeam, MultiTeam }

public class Document
{
    public Guid Id { get; set; }
    public string RepoPath { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string? Team { get; set; }
    public string? Owner { get; set; }
    public string? Approver { get; set; }
    public DocumentStatus Status { get; set; } = DocumentStatus.Draft;
    public DocumentScope Scope { get; set; } = DocumentScope.SingleTeam;
    public string Tags { get; set; } = "[]"; // jsonb
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public string? CurrentCommit { get; set; }
    public string? BaseReviewCommit { get; set; }
    public ICollection<ReviewParticipant> Participants { get; set; } = new List<ReviewParticipant>();
}
