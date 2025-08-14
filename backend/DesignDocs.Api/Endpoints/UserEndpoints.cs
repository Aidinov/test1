namespace DesignDocs.Api.Endpoints;

public static class UserEndpoints
{
    public static void MapUserEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapGet("/api/me", (HttpContext ctx) =>
        {
            var user = ctx.Request.Headers["X-User"].FirstOrDefault() ?? "anonymous";
            return Results.Ok(new { Login = user });
        });
    }
}
