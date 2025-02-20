﻿namespace Zomp.SyncMethodGenerator.Tests;

public static class TestHelper
{
    public static Task Verify(string source, bool uniqueForFramework = false)
    {
        var preprocessorSymbols = new string[]
        {
#if NET7_0
            "NET7_0",
#endif
#if NET7_0_OR_GREATER
            "NET7_0_OR_GREATER",
#endif
#if NET6_0
            "NET6_0",
#endif
#if NET6_0_OR_GREATER
            "NET6_0_OR_GREATER",
#endif
        };

        var parseOptions = CSharpParseOptions.Default
            .WithLanguageVersion(LanguageVersion.Preview)
            .WithPreprocessorSymbols(preprocessorSymbols);

        SyntaxTree syntaxTree = CSharpSyntaxTree.ParseText(source, parseOptions);
        //SyntaxTree syntaxTree = CSharpSyntaxTree.ParseText(source);

        var locations = new[] {
            typeof(IAsyncEnumerable<>).Assembly.Location,
            typeof(System.Drawing.Point).Assembly.Location,
            typeof(object).Assembly.Location,
            typeof(Console).Assembly.Location,
            typeof(Memory<>).Assembly.Location,
            typeof(Queue<>).Assembly.Location,
            typeof(LinkedListNode<>).Assembly.Location,
        };

        var distinct = locations.Distinct().ToArray();

        IEnumerable<PortableExecutableReference> references = distinct
            .Select(l => MetadataReference.CreateFromFile(l));

        CSharpCompilation compilation = CSharpCompilation.Create(
            assemblyName: "Tests",
            options: new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary),
            syntaxTrees: new[] { syntaxTree },
            references: references);

        var generator = new SyncMethodSourceGenerator();

        GeneratorDriver driver = CSharpGeneratorDriver.Create(generator).WithUpdatedParseOptions(parseOptions);

        driver = driver.RunGenerators(compilation);

        // Ensure that at least two sources are generated
        var results = driver.GetRunResult();
        if (results.Diagnostics.Length == 0 && results.GeneratedTrees.Length < 2)
        {
            throw new InvalidOperationException("Nothing generated");
        }

        var verifier = Verifier
            .Verify(driver)
            .UseDirectory("Snapshots");

        if (uniqueForFramework)
        {
            verifier = verifier.UniqueForTargetFrameworkAndVersion();
        }

        return verifier;
    }
}
