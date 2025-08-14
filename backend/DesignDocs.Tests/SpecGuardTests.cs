using System;
using System.IO;
using System.Linq;
using System.Text.Json;
using Mono.Cecil;
using Mono.Cecil.Cil;
using Xunit;

namespace DesignDocs.Tests;

public class SpecGuardTests
{
    // Какие сборки считаем «оригинальным проектом»:
    private static readonly string[] ProductAssemblyNames =
    [
        "DesignDocs.Api",
    ];

    // Опционально: белый список неймспейсов продукта
    private static readonly string[] ProductNamespacesStartsWith =
    [
        
    ];

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
                .Where(a => a.AttributeType.FullName == "Xunit.TraitAttribute" && a.ConstructorArguments.Count == 2 &&
                            (string)a.ConstructorArguments[0].Value! == "Spec")
                .Select(a => (string)a.ConstructorArguments[1].Value!))
            .ToHashSet();
        Assert.True(manifest.SetEquals(traitIds),
            $"Manifest IDs: {string.Join(',', manifest)} Traits: {string.Join(',', traitIds)}");
    }

    [Fact]
    public void EachSpecTest_CallsIntoProductAssemblies()
    {
        // Загружаем тестовую сборку (ту, где этот тест выполняется)
        var testAsm = typeof(SpecGuardTests).Assembly;
        var testAsmPath = testAsm.Location;

        // Загружаем через Cecil для доступа к IL
        var resolver = new DefaultAssemblyResolver();
        resolver.AddSearchDirectory(Path.GetDirectoryName(testAsmPath)!);

        var readerParams = new ReaderParameters { AssemblyResolver = resolver, ReadSymbols = true };
        var module = ModuleDefinition.ReadModule(testAsmPath, readerParams);

        // Находим все методы-тесты с Trait("Spec", ...)
        var testMethods = module.Types
            .SelectMany(t => t.Methods)
            .Where(m => m.HasCustomAttributes &&
                        m.CustomAttributes.Any(a =>
                            a.AttributeType.FullName is "Xunit.FactAttribute" or "Xunit.TheoryAttribute") &&
                        m.CustomAttributes.Any(a =>
                            a.AttributeType.FullName == "Xunit.TraitAttribute" &&
                            a.ConstructorArguments.Count == 2 &&
                            (string)a.ConstructorArguments[0].Value! == "Spec"))
            .ToList();

        Assert.NotEmpty(testMethods); // на случай пустого набора

        var offenders = testMethods
            .Where(m => !CallsProductCode(m))
            .Select(m => $"{m.DeclaringType.FullName}.{m.Name}")
            .ToList();

        Assert.True(offenders.Count == 0,
            "Найдены тесты со Spec, которые не вызывают код продукта: " + string.Join(", ", offenders));
    }

    private static bool CallsProductCode(MethodDefinition method)
    {
        if (!method.HasBody) return false;
        foreach (var ins in method.Body.Instructions)
        {
            if (ins.OpCode == OpCodes.Call || ins.OpCode == OpCodes.Callvirt || ins.OpCode == OpCodes.Newobj)
            {
                var mr = ins.Operand as MethodReference;
                if (mr == null) continue;

                // Разрешаем метод, чтобы узнать сборку и полное имя типа
                var declaringType = mr.DeclaringType?.Resolve();
                if (declaringType == null) continue;

                var asmName = declaringType.Module.Assembly?.Name?.Name;
                var fullTypeName = declaringType.FullName?.Replace('/', '.');

                // Условие «внешнего продуктового вызова»
                var matchesAssembly = asmName != null && ProductAssemblyNames.Contains(asmName);
                var matchesNamespace = fullTypeName != null &&
                                       ProductNamespacesStartsWith.Any(ns =>
                                           fullTypeName.StartsWith(ns, StringComparison.Ordinal));

                if (matchesAssembly || matchesNamespace)
                    return true;
            }
        }

        return false;
    }
}