using System;
using System.IO;
using System.Linq;
using System.Text.Json;
using Xunit;

namespace DesignDocs.Tests;

public class SpecGuardTests
{
    [Fact]
    public void ManifestMatchesTraits()
    {
        var root = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "../../../../.."));
        var manifestPath = Path.Combine(root, "spec", "spec-manifest.backend.json");
        var manifest = JsonSerializer.Deserialize<string[]>(File.ReadAllText(manifestPath))!
            .ToHashSet();
        var traitIds = typeof(SpecGuardTests).Assembly.GetTypes()
            .SelectMany(t => t.GetMethods())
            .SelectMany(m => m.GetCustomAttributesData()
                .Where(a => a.AttributeType.FullName == "Xunit.TraitAttribute" && a.ConstructorArguments.Count == 2 && (string)a.ConstructorArguments[0].Value! == "Spec")
                .Select(a => (string)a.ConstructorArguments[1].Value!))
            .ToHashSet();
        Assert.True(manifest.SetEquals(traitIds),
            $"Manifest IDs: {string.Join(',', manifest)} Traits: {string.Join(',', traitIds)}");
    }
}
