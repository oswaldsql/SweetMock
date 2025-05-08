namespace SweetMock.BuilderTests.Util;

public static class AssertExtensions
{
    public static bool HasErrors(this Diagnostic[] diagnostics) => diagnostics.Any(t => t.Severity == DiagnosticSeverity.Error);
    public static bool HasNoErrors(this Diagnostic[] diagnostics) => diagnostics.All(t => t.Severity < DiagnosticSeverity.Error);
    public static bool HasNoWarnings(this Diagnostic[] diagnostics) => diagnostics.All(t => t.Severity < DiagnosticSeverity.Warning);

    public static Diagnostic[] GetErrors(this Diagnostic[] diagnostics) => diagnostics.Where(t => t.Severity == DiagnosticSeverity.Error).ToArray();
    public static Diagnostic[] GetErrors(this (SyntaxTree[] syntaxTrees, Diagnostic[] diagnostics) source) => source.diagnostics.Where(t => t.Severity >= DiagnosticSeverity.Error).ToArray();
    public static Diagnostic[] GetWarnings(this (SyntaxTree[] syntaxTrees, Diagnostic[] diagnostics) source) => source.diagnostics.Where(t => t.Severity >= DiagnosticSeverity.Warning).ToArray();

    public static IEnumerable<string> GetFileContent(this (SyntaxTree[] syntaxTrees, Diagnostic[] diagnostics) source, string filePathPart) => source.syntaxTrees.Where(t => t.FilePath.Contains(filePathPart)).Select(t => t.GetText().ToString());
}
