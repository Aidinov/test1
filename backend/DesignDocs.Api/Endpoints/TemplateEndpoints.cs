namespace DesignDocs.Api.Endpoints;

public static class TemplateEndpoints
{
    public static void MapTemplateEndpoints(this IEndpointRouteBuilder app)
    {
        var templatesDir = Path.Combine(Directory.GetCurrentDirectory(), "docs", "templates");

        app.MapGet("/api/templates", () =>
        {
            if (!Directory.Exists(templatesDir)) return Enumerable.Empty<string>();
            return Directory.GetFiles(templatesDir, "*.md").Select(f => Path.GetFileNameWithoutExtension(f));
        });

        app.MapGet("/api/templates/{name}", (string name) =>
        {
            var path = Path.Combine(templatesDir, name + ".md");
            return File.Exists(path) ? Results.Text(File.ReadAllText(path), "text/markdown") : Results.NotFound();
        });
    }
}
