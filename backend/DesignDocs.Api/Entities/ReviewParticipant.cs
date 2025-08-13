namespace DesignDocs.Api.Entities;

public enum ParticipantRole { Author, Reviewer, Approver, Moderator, Observer }

public class ReviewParticipant
{
    public Guid Id { get; set; }
    public Guid DocumentId { get; set; }
    public Document? Document { get; set; }
    public string UserId { get; set; } = string.Empty;
    public ParticipantRole Role { get; set; }
}
