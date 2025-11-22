namespace SweetMock.BuilderTests.Util;

using Xunit.Abstractions;

public static class DumpExtensions
{
    public static void DumpResult(this ITestOutputHelper output,
        (SyntaxTree[] syntaxTrees, Diagnostic[] diagnostics) source) =>
        output.DumpResult(source.syntaxTrees, source.diagnostics);

    public static void DumpResult(this ITestOutputHelper output, SyntaxTree[] syntaxTrees, Diagnostic[] diagnostics)
    {
        if (diagnostics.Length == 0)
        {
            output.WriteLine("No diagnostics found");
            return;
        }
        else
        {
            output.WriteLine("--- diagnostics ---");

            foreach (var item in diagnostics.OrderBy(t=> t.Severity).ThenBy(t => t.ToString()))
            {
                output.WriteLine("-- " + item.Severity + " | " + item.Id + " --");
                output.WriteLine($"{item.GetMessage()}[{item.Location.SourceTree.FilePath} [{item.Location.GetLineSpan().StartLinePosition} - {item.Location.GetLineSpan().EndLinePosition}]");
                output.WriteLine(item.GetCode());
                output.WriteLine("");
            }

            output.WriteLine("");
            return;
        }

        foreach (var syntaxTree in syntaxTrees.Where(t => !t.FilePath.Contains(".BaseFiles.")))
        {
            output.WriteLine("--- File : " + syntaxTree.FilePath + " ---");

            var t = syntaxTree.ToString().Split("\r\n");
            for (var i = 0; i < t.Length; i++)
            {
                output.WriteLine($"{i + 1:D5} {t[i]}");
            }

            output.WriteLine("");
        }
    }

    public static string GetCode(this Diagnostic actual) => actual.Location.GetCode2();

    public static string GetCode(this Location location)
    {
        var start = location.SourceSpan.Start;
        var length = location.SourceSpan.Length;
        return location.SourceTree?.ToString().Substring(start, length) ?? "";
    }
    
    public static string GetCode2(this Location location)
    {
        var sourceText = location.SourceTree?.ToString();
        if (sourceText == null) return "";
            
        var lines = sourceText.Split("\r\n");
        var span = location.SourceSpan;
            
        var start = span.Start;
        var length = span.Length;

        var startLine = location.GetLineSpan().StartLinePosition.Line;
        var endLine = location.GetLineSpan().EndLinePosition.Line;

        var beforeLines = Math.Max(startLine - 2, 0);
        var afterLines = Math.Min(endLine + 2, lines.Length - 1);

        var result = new List<string>();
        for (int i = beforeLines; i <= afterLines; i++)
        {
            result.Add(i.ToString("0000") + " " + lines[i]);
        }

        return string.Join("\r\n", result);
    }
}
