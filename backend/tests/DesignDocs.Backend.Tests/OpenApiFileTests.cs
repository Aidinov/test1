using System.Text.Json;
using System.IO;
using Xunit;

namespace DesignDocs.Backend.Tests;

public class OpenApiFileTests
{
    [Fact]
    public void SwaggerJsonLoads()
    {
        var root = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "../../../../../.."));
        var path = Path.Combine(root, "backend", "swagger.json");
        Assert.True(File.Exists(path));
        var json = JsonSerializer.Deserialize<JsonElement>(File.ReadAllText(path));
        Assert.True(json.GetProperty("openapi").GetString()?.StartsWith("3.") ?? false);
    }
}
