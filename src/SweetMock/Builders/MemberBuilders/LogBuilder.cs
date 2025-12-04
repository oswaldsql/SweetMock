namespace SweetMock.Builders.MemberBuilders;

using Generation;
using Utils;

public static class LogBuilder
{
    internal static void InitializeLogging(this CodeBuilder builder, MockContext context) =>
        builder.Region("Logging", builder => builder
            .Add("private global::SweetMock.CallLog _sweetMockCallLog = new global::SweetMock.CallLog();").BR()
            .Add("private void _log(global::SweetMock.ArgumentBase argument) => this._sweetMockCallLog.Add(argument);").BR()
            .AddToConfig(context, codeBuilder => codeBuilder
                .Scope($"internal MockConfig GetCallLogs(out {context.Name}_Logs callLog)", builder1 => builder1
                    .Add($"callLog = new {context.Name}_Logs(this.target._sweetMockCallLog, this.target._sweetMockInstanceName);")
                    .Add("return this;"))
            )
        );

    internal static CodeBuilder BuildLogExtensionsClass(this CodeBuilder builder, MockContext context)
    {
        var memberGroups = context
            .GetCandidates()
            .Distinct(SymbolEqualityComparer.IncludeNullability)
            .ToLookup(t => t.Name);

        builder
            .Scope($"internal partial class MockOf_{context.Name}{context.Source.GetTypeGenerics()}", c => c
                .Scope($"internal class {context.Name}_Logs(CallLog log, string? instanceName = null)", classScope =>
                {
                    classScope
                        .Add($"public System.Collections.Generic.IEnumerable<ArgumentBase> All() =>")
                        .Add($"    log.Calls.Where(t => instanceName == null || t.InstanceName == instanceName);");

                    foreach (var g in memberGroups)
                    {
                        var gKey = GenerateLogKey(context, g);

                        var argsClass = $"{context.MockName}{context.Source.GetTypeGenerics()}.{gKey}_Arguments";

                        classScope
                            .BR()
                            .Add($"public System.Collections.Generic.IEnumerable<{argsClass}> {gKey}(System.Func<{argsClass}, bool>? filter = null) =>")
                            .Add($"    this.All().OfType<{argsClass}>().Where(filter ?? (_ => true));");
                    }
                })
            )
            .BR();

        return builder;
    }

    private static string GenerateLogKey(MockContext context, IGrouping<string, ISymbol> g) =>
        g.Key switch
        {
            "this[]" => "Indexer",
            ".ctor" => context.Name,
            _ => g.Key
        };

    public static string GenerateKeyType(this IPropertySymbol[] symbols)
    {
        var hasMultipleTypes = symbols.Select(t => t.Type).Distinct(SymbolEqualityComparer.Default).Count() > 1;
        if (hasMultipleTypes)
        {
            return "global::System.Object?";
        }

        var type = symbols.First().Parameters.First().Type;
        return DetermineArgumentType(type);
    }

    public static string GenerateReturnType(this IPropertySymbol[] symbols)
    {
        var hasMultipleTypes = symbols.Select(t => t.Type).Distinct(SymbolEqualityComparer.Default).Count() > 1;
        if (hasMultipleTypes)
        {
            return $"global::System.Object?";
        }

        var type = symbols.First().Type;
        return DetermineArgumentType(type);
    }

    public static string DetermineArgumentType(ITypeSymbol type)
    {
        if (type is ITypeParameterSymbol)
        {
            return "global::System.Object?";
        }

        if (type is INamedTypeSymbol namedType && namedType.IsGenericType)
        {
            return "global::System.Object?";
        }

        if (type.NullableAnnotation != NullableAnnotation.Annotated)
        {
            return type.ToDisplayString(Format.ExtendedTypeFormat) + "?";
        }

        return type.ToDisplayString(Format.ExtendedTypeFormat);
    }

    public static string GenerateArgumentDeclaration(this IGrouping<string, IParameterSymbol> argument)
    {
        var hasMultipleTypes = argument.Select(t => t.Type).Distinct(SymbolEqualityComparer.Default).Count() > 1;
        if (hasMultipleTypes)
        {
            return $"global::System.Object? {argument.Key} = null";
        }

        var firstType = argument.First().Type;
        return firstType switch
        {
            INamedTypeSymbol { DelegateInvokeMethod: not null } => $"global::System.Object? {argument.Key} = null",
            INamedTypeSymbol namedType when namedType.TypeArguments.Length > 0 || namedType.IsGenericType => $"{namedType.WithNullableAnnotation(NullableAnnotation.Annotated).ToDisplayString(Format.ExtendedTypeFormat)} {argument.Key} = null",
            INamedTypeSymbol => $"{firstType.ToDisplayString(Format.ExtendedTypeFormat)}? {argument.Key} = null",
            ITypeParameterSymbol => $"global::System.Object? {argument.Key} = null",
            _ => $"{firstType.WithNullableAnnotation(NullableAnnotation.Annotated).ToDisplayString(Format.ExtendedTypeFormat)}? {argument.Key} = null"
        };
    }
}
