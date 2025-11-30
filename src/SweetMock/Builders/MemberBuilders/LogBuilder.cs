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
                    .Scope($"internal MockConfig GetCallLogs(out {context.Source.Name}_Logs callLog)", builder1 => builder1
                        .Add($"callLog = new {context.Source.Name}_Logs(target._sweetMockCallLog, target._sweetMockInstanceName);")
                        .Add("return this;"))
                )
            );
    }

    private static readonly SymbolDisplayFormat MethodSignatureFormat = new(globalNamespaceStyle: SymbolDisplayGlobalNamespaceStyle.Omitted, typeQualificationStyle: SymbolDisplayTypeQualificationStyle.NameAndContainingTypes, genericsOptions: SymbolDisplayGenericsOptions.IncludeTypeConstraints, memberOptions: SymbolDisplayMemberOptions.IncludeParameters, parameterOptions: SymbolDisplayParameterOptions.IncludeType);

    private static readonly SymbolDisplayFormat FullyQualifiedFormat = SymbolDisplayFormat.FullyQualifiedFormat;

    private static string MethodSignature(MockContext context, IMethodSymbol symbol) =>
        $"{context.Source.ToDisplayString(FullyQualifiedFormat)}.{symbol.ToDisplayString(MethodSignatureFormat)}";

    private static string Argument(IParameterSymbol t, int i) =>
        i == 0 ? $".With(\"{t.Name}\", {t.Name})" : $".And(\"{t.Name}\", {t.Name})";
}
