using DesignDocs.Api.Entities;

namespace DesignDocs.Api.Models;

public record ReviewStartRequest(Guid DocumentId, string BaseCommit, List<ReviewParticipantDto> Participants);
public record ReviewParticipantDto(string UserId, ParticipantRole Role);
