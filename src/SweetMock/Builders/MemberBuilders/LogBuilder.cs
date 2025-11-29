namespace SweetMock.Builders.MemberBuilders;

using Generation;
using Utils;

public static class LogBuilder
{
    extension(CodeBuilder builder)
    {
        internal void InitializeLogging(MockContext context) =>
            builder.Region("Logging", builder => builder
                .Add("private global::SweetMock.CallLog _sweetMockCallLog = new global::SweetMock.CallLog();")
                .Add("private void _log(global::SweetMock.ArgumentBase argument) {_sweetMockCallLog.Calls.Add(argument);}")
                .AddToConfig(context, codeBuilder => codeBuilder
                    .Scope($"internal MockConfig GetCallLogs(out {context.Source.Name}_Logs{context.Source.GetTypeGenerics()} callLog)", builder1 => builder1
                        .Add($"callLog = new {context.Source.Name}_Logs{context.Source.GetTypeGenerics()}(target._sweetMockCallLog, target._sweetMockInstanceName);")
                        .Add("return this;"))
                )
            );

        internal CodeBuilder BuildLogSegment(MockContext context, IMethodSymbol? symbol, bool skipParameters = false)
        {
            return builder;

            if (symbol == null) { return builder; }

            if (!skipParameters && symbol.Parameters.Any(t => t.RefKind == RefKind.None))
            {
                var LogArgumentsValue = symbol.Parameters.Where(t => t.RefKind == RefKind.None).Select(Argument);
                var logArgs = string.Join("", LogArgumentsValue);
                builder.Scope("if(_sweetMockCallLog != null)", b => b
                    .Add($"_sweetMockCallLog.Add(\"{MethodSignature(context,symbol)}\", global::SweetMock.Arguments{logArgs});"));
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
