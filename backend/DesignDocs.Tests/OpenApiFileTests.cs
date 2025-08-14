using System;
using System.IO;
using System.Text.Json;
using Xunit;

namespace DesignDocs.Tests;

public class OpenApiFileTests
{
    [Fact]
    public void SwaggerJsonLoads()
    {
        var root = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "../../../.."));
        var path = Path.Combine(root, "swagger.json");
        Assert.True(File.Exists(path));
        var json = JsonSerializer.Deserialize<JsonElement>(File.ReadAllText(path));
        Assert.True(json.GetProperty("openapi").GetString()?.StartsWith("3.") ?? false);
    }
}
