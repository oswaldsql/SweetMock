namespace SweetMock.Builders.MemberBuilders;

using Generation;

public static class LogBuilder
{
    extension(CodeBuilder builder)
    {
        internal void InitializeLogging() =>
            builder.Region("Logging", builder => builder
                .Add("private SweetMock.CallLog? _sweetMockCallLog = new SweetMock.CallLog();"));

        internal CodeBuilder BuildLogSegment(MockContext context, IMethodSymbol? symbol, bool skipParameters = false)
        {
            if (symbol == null) { return builder; }

            if (!skipParameters && symbol.Parameters.Any(t => t.RefKind == RefKind.None))
            {
                var LogArgumentsValue = symbol.Parameters.Where(t => t.RefKind == RefKind.None).Select(Argument);
                var logArgs = string.Join("", LogArgumentsValue);
                builder.Scope("if(_sweetMockCallLog != null)", b => b
                    .Add($"_sweetMockCallLog.Add(\"{MethodSignature(context,symbol)}\", SweetMock.Arguments{logArgs});"));
            }
            else
            {
                builder.Scope("if(_sweetMockCallLog != null)", b => b
                    .Add($"_sweetMockCallLog.Add(\"{MethodSignature(context,symbol)}\");"));
            }

            return builder;
        }
    }

    private static readonly SymbolDisplayFormat MethodSignatureFormat = new(globalNamespaceStyle: SymbolDisplayGlobalNamespaceStyle.Omitted, typeQualificationStyle: SymbolDisplayTypeQualificationStyle.NameAndContainingTypes, genericsOptions: SymbolDisplayGenericsOptions.IncludeTypeConstraints, memberOptions: SymbolDisplayMemberOptions.IncludeParameters, parameterOptions: SymbolDisplayParameterOptions.IncludeType);

    private static readonly SymbolDisplayFormat FullyQualifiedFormat = SymbolDisplayFormat.FullyQualifiedFormat;

    private static string MethodSignature(MockContext context, IMethodSymbol symbol) =>
        $"{context.Source.ToDisplayString(FullyQualifiedFormat)}.{symbol.ToDisplayString(MethodSignatureFormat)}";

    private static string Argument(IParameterSymbol t, int i) =>
        i == 0 ? $".With(\"{t.Name}\", {t.Name})" : $".And(\"{t.Name}\", {t.Name})";
}
