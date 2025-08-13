using Microsoft.AspNetCore.Mvc;

namespace DesignDocs.Api.Endpoints;

public static class GitlabWebhookEndpoints
{
    public static void MapGitlabWebhookEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapPost("/api/gitlab/webhook", ([FromHeader(Name = "X-Gitlab-Token")] string token, IConfiguration cfg) =>
        {
            var secret = cfg["GITLAB_WEBHOOK_SECRET"];
            if (token != secret) return Results.Unauthorized();
            return Results.Ok();
        });
    }
}
