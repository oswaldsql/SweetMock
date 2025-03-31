namespace SweetMock.Builders;

using System.Linq;
using Microsoft.CodeAnalysis;
using Utils;

public static class LogBuilder
{
    internal static CodeBuilder InitializeLogging(this CodeBuilder source)
    {
        using (source.Region("Logging"))
        {
            source.Add("""
                       private bool _hasLog = false;
                       private SweetMock.CallLog _log = new SweetMock.CallLog();
                       """);
            using (source.AddToConfig())
            {
                source.AddSummary("Add logging to the configuration.");
                source.AddParameter("callLog", "CallLog to use for logging.");
                source.AddReturns("The configuration object.");
                source.Add("""
                           public Config LogCallsTo(SweetMock.CallLog callLog) {
                                 target._log = callLog;
                                 target._hasLog = true;
                                 return this;
                           }
                           """);
            }
        }

        return source;
    }

    public static string BuildLogSegment(IMethodSymbol? symbol, bool skipParameters = false)
    {
        if (symbol == null) { return ""; }

        CodeBuilder result = new();
        BuildLogSegment(result, symbol, skipParameters);
        return result.ToString();
    }

    internal static CodeBuilder BuildLogSegment(this CodeBuilder builder, IMethodSymbol? symbol, bool skipParameters = false)
    {
        if (symbol == null) { return builder; }

        if (!skipParameters && symbol.Parameters.Any(t => t.RefKind == RefKind.None))
        {
            var LogArgumentsValue = symbol.Parameters.Where(t => t.RefKind == RefKind.None).Select(argument);
            builder.Scope("if(_hasLog)", b => b.Add($"_log.Add(\"{symbol}\", SweetMock.Arguments{string.Join("", LogArgumentsValue)});"));
        }
        else
        {
            builder.Scope("if(_hasLog)", b => b.Add($"_log.Add(\"{symbol}\");"));
        }

        return builder;
    }

    private static string argument(IParameterSymbol t, int i) => i == 0 ? $".With(\"{t.Name}\", {t.Name})" : $".And(\"{t.Name}\", {t.Name})";
}
