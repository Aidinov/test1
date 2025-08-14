namespace DesignDocs.Api.Models;

public record DocumentCreateRequest(string Template, string Title, string? Team, string? Owner, string? PathHints);
public record DocumentContentRequest(string Content);
