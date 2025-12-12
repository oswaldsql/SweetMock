namespace SweetMock.BuilderTests.Util;

using Xunit.Abstractions;

public static class DumpExtensions
{
    public static void DumpResult(this ITestOutputHelper output,
        (SyntaxTree[] syntaxTrees, Diagnostic[] diagnostics) source) =>
        output.DumpResult(source.diagnostics);

    public static void DumpResult(this ITestOutputHelper output, Diagnostic[] diagnostics)
    {
        if (diagnostics.Length == 0)
        {
            output.WriteLine("No diagnostics found");
            return;
        }
        
        output.WriteLine("--- diagnostics ---");

        foreach (var item in diagnostics.OrderBy(t=> t.Severity).ThenBy(t => t.ToString()))
        {
            output.WriteLine("-- " + item.Severity + " | " + item.Id + " --");
            output.WriteLine($"{item.GetMessage()}[{item.Location.SourceTree!.FilePath} [{item.Location.GetLineSpan().StartLinePosition} - {item.Location.GetLineSpan().EndLinePosition}]");
            output.WriteLine(item.GetCode());

            if (item.AdditionalLocations.Count>0)
            {
                output.WriteLine("-- also at --");
                foreach (var location in item.AdditionalLocations)
                {
                    output.WriteLine(location.ToString());
                }
            }
            
            output.WriteLine("");
        }

        output.WriteLine("");
    }

    private static string GetCode(this Diagnostic actual)
    {
        var sourceText = actual.Location.SourceTree?.ToString();
        if (sourceText == null) return "";
            
        var lines = sourceText.Split("\r\n");
            
        var startLine = actual.Location.GetLineSpan().StartLinePosition.Line;
        var endLine = actual.Location.GetLineSpan().EndLinePosition.Line;

        var beforeLines = Math.Max(startLine - 2, 0);
        var afterLines = Math.Min(endLine + 2, lines.Length - 1);

        var result = new List<string>();
        for (var i = beforeLines; i <= afterLines; i++)
        {
            result.Add(i.ToString("0000") + " " + lines[i]);
        }

        return string.Join("\r\n", result);
    }

    public static string GetCode(this Location location)
    {
        var start = location.SourceSpan.Start;
        var length = location.SourceSpan.Length;
        return location.SourceTree?.ToString().Substring(start, length) ?? "";
    }
}
