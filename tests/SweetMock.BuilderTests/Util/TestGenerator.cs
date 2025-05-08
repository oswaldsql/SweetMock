namespace SweetMock.BuilderTests.Util;

using Microsoft.CodeAnalysis.CSharp;
using System.Linq;

public static class TestGenerator
{
    private const LanguageVersion Language = LanguageVersion.CSharp12;

    public static (SyntaxTree[] syntaxTrees, Diagnostic[] diagnostics) Generate<T>(params string[] sourceCode)
        where T : IIncrementalGenerator, new() => new T().Generate(sourceCode);

    public static (SyntaxTree[] syntaxTrees, Diagnostic[] diagnostics) Generate(this IIncrementalGenerator generator,
        params string[] sourceCode)
    {
        var references = AppDomain.CurrentDomain.GetAssemblies()
            .Where(assembly => !assembly.IsDynamic)
            .Where(assembly => !assembly.FullName!.StartsWith("SweetMock,"))
            .Select(assembly => MetadataReference.CreateFromFile(assembly.Location))
            .Cast<MetadataReference>();

        var syntaxTree =
            sourceCode.Select((t, index) =>
                CSharpSyntaxTree.ParseText(t, path: $"source{index}.Input.cs", options: new CSharpParseOptions(Language)));

        var options = new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary);

        var compilation = CSharpCompilation.Create("GeneratedAssembly",
            syntaxTree,
            references,
            options);

        CSharpGeneratorDriver.Create(generator).WithUpdatedParseOptions(new CSharpParseOptions(Language))
            .RunGeneratorsAndUpdateCompilation(compilation,
                out var outputCompilation, out var diagnosticsResult);

        var syntaxTrees = outputCompilation.SyntaxTrees
            .Where(t => !t.FilePath.EndsWith(".Input.cs")).ToArray();

        var success = outputCompilation.Emit(new MemoryStream());
        var diagnostics = success.Diagnostics.Where(t => t.Severity > DiagnosticSeverity.Hidden)
            .Concat(diagnosticsResult).ToArray();

        return (syntaxTrees, diagnostics);
    }
}