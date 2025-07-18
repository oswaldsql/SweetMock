namespace SweetMock.Builders;

using Generation;

public static class LogBuilder
{
    internal static void InitializeLogging(this CodeBuilder source) =>
        source.Region("Logging", builder =>
        {
            builder
                .Add("private SweetMock.CallLog? _sweetMockCallLog = new SweetMock.CallLog();");
        });

    internal static CodeBuilder BuildLogSegment(this CodeBuilder builder, IMethodSymbol? symbol, bool skipParameters = false)
    {
        if (symbol == null) { return builder; }

        if (!skipParameters && symbol.Parameters.Any(t => t.RefKind == RefKind.None))
        {
            var LogArgumentsValue = symbol.Parameters.Where(t => t.RefKind == RefKind.None).Select(Argument);
            var logArgs = string.Join("", LogArgumentsValue);
            builder.Scope("if(_sweetMockCallLog != null)", b => b
                .Add($"_sweetMockCallLog.Add(\"{symbol}\", SweetMock.Arguments{logArgs});"));
        }
        else
        {
            builder.Scope("if(_sweetMockCallLog != null)", b => b
                .Add($"_sweetMockCallLog.Add(\"{symbol}\");"));
        }

        return builder;
    }

    private static string Argument(IParameterSymbol t, int i) =>
        i == 0 ? $".With(\"{t.Name}\", {t.Name})" : $".And(\"{t.Name}\", {t.Name})";
}
