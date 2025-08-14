using DesignDocs.Api.Entities;

namespace DesignDocs.Api.Models;

public record CommentCreateRequest(Guid DocumentId, string AuthorId, CommentType Type, CommentSeverity Severity, CommentAnchorRequest Anchor);
public record CommentAnchorRequest(string BaseCommit, string BlockPath, int StartOffset, int EndOffset, string TextPrefix, string TextQuote, string TextSuffix, string BlockHash);
public record CommentMessageRequest(string AuthorId, string Body, Guid? ParentMessageId);
public record CommentResolveRequest(CommentResolution Resolution, string? Note, string? Commit);
public record CommentPatchRequest(CommentType? Type, CommentSeverity? Severity);
