namespace SweetMock.Builders;

using System.Linq;
using Generation;
using Microsoft.CodeAnalysis;

public static class LogBuilder
{
    internal static void InitializeLogging(this CodeBuilder source) =>
        source.Region("Logging", builder =>
        {
            builder.Add("private bool _hasLog = false;")
                .Add("private SweetMock.CallLog _log = new SweetMock.CallLog();");

            builder.AddToConfig(config =>
            {
                config.Documentation(doc => doc
                    .Summary("Add logging to the configuration.")
                    .Parameter("callLog", "CallLog to use for logging.")
                    .Returns("The configuration object."));

                config.AddConfigMethod("LogCallsTo", ["SweetMock.CallLog callLog"], codeBuilder => codeBuilder
                    .Add("target._log = callLog;")
                    .Add("target._hasLog = true;")
                );
            });
        });

    internal static CodeBuilder BuildLogSegment(this CodeBuilder builder, IMethodSymbol? symbol, bool skipParameters = false)
    {
        if (symbol == null) { return builder; }

        if (!skipParameters && symbol.Parameters.Any(t => t.RefKind == RefKind.None))
        {
            var LogArgumentsValue = symbol.Parameters.Where(t => t.RefKind == RefKind.None).Select(Argument);
            builder.Scope("if(_hasLog)", b => b.Add($"_log.Add(\"{symbol}\", SweetMock.Arguments{string.Join("", LogArgumentsValue)});"));
        }
        else
        {
            builder.Scope("if(_hasLog)", b => b.Add($"_log.Add(\"{symbol}\");"));
        }

        return builder;
    }

    private static string Argument(IParameterSymbol t, int i) => i == 0 ? $".With(\"{t.Name}\", {t.Name})" : $".And(\"{t.Name}\", {t.Name})";
}
