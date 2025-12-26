namespace SweetMock.Builders.MemberBuilders;

using Generation;
using Utils;

internal static class LogBuilder
{
    internal static void InitializeLogging(this CodeBuilder builder, MockInfo mock) =>
        builder.Region("Logging", regionScope => regionScope
            .Add("private global::SweetMock.CallLog _sweetMockCallLog = new global::SweetMock.CallLog();").BR()
            .Add("private void _log(global::SweetMock.ArgumentBase argument) => this._sweetMockCallLog.Add(argument);").BR()
            .AddToConfig(mock, configScope => configScope
                .Scope($"internal MockConfig GetCallLogs(out {mock.Name}_Logs callLog)", builder1 => builder1
                    .Add($"callLog = new {mock.Name}_Logs(this.target._sweetMockCallLog, this.target._sweetMockInstanceName);")
                    .Add("return this;"))
            )
        );

    internal static CodeBuilder BuildLogExtensionsClass(this CodeBuilder builder, MockInfo mock)
    {
        var memberGroups = mock
            .Candidates
            .Distinct(SymbolEqualityComparer.IncludeNullability)
            .ToLookup(t => t.Name);

        builder
            .Scope($"internal partial class MockOf_{mock.Name}{mock.Generics}", c => c
                .Scope($"internal class {mock.Name}_Logs(CallLog log, string? instanceName = null)", classScope =>
                {
                    classScope
                        .Add("public System.Collections.Generic.IEnumerable<ArgumentBase> All() =>")
                        .Add("    log.Calls.Where(t => instanceName == null || t.InstanceName == instanceName);");

                    foreach (var logKey in memberGroups.Select(t => GenerateLogKey(mock, t)))
                    {
                        var argsClass = $"{mock.MockName}{mock.Generics}.{logKey}_Arguments";

                        classScope
                            .BR()
                            .Add($"public global::System.Collections.Generic.IEnumerable<{argsClass}> {logKey}(System.Func<{argsClass}, bool>? filter = null) =>")
                            .Add($"    this.All().OfType<{argsClass}>().Where(filter ?? (_ => true));");
                    }
                })
            )
            .BR();

        builder
            .Scope($"internal static class {mock.MockName}_LogExtensions", classScope =>
            {
                classScope
                    .Add($"public static {mock.MockName}{mock.Generics}.{mock.Name}_Logs {mock.Name}{mock.Generics}(this CallLog all){mock.Constraints} => new(all);");

                foreach (var logKey in memberGroups.Select(t => GenerateLogKey(mock, t)))
                {
                    classScope.Add($"public static global::System.Collections.Generic.IEnumerable<{mock.MockName}{mock.Generics}.{logKey}_Arguments> {logKey}{mock.Generics}(this global::SweetMock.CallLog all, global::System.Func<{mock.MockName}{mock.Generics}.{logKey}_Arguments, bool>? filter = null){mock.Constraints} => all.{mock.Name}{mock.Generics}().{logKey}(filter);");
                }
            });

        return builder;
    }

    private static string GenerateLogKey(MockInfo mock, IGrouping<string, ISymbol> g) =>
        g.Key switch
        {
            "this[]" => "Indexer",
            ".ctor" => mock.Name,
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
            return "global::System.Object?";
        }

        var type = symbols.First().Type;
        return DetermineArgumentType(type);
    }

    internal static string GenerateReturnType(this IEnumerable<PropertyBuilder.PropertyMetadata> symbols)
    {
        var distinct = symbols.Select(t => t.Symbol.Type).Distinct(TypeSymbolEqualityComparer.Default).ToArray();

        var hasMultipleTypes = distinct.Length > 1;
        if (hasMultipleTypes)
        {
            return "global::System.Object?";
        }

        var type = distinct.First();
        return DetermineArgumentType(type);
    }

    public static string DetermineArgumentType(ITypeSymbol type)
    {
        if (type is INamedTypeSymbol { IsGenericType: true })
        {
            return "global::System.Object?";
        }

        if (type is ITypeParameterSymbol)
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
            INamedTypeSymbol namedType when namedType.TypeArguments.Length > 0 || namedType.IsGenericType => $"{namedType.AsNullable()} {argument.Key} = null",
            INamedTypeSymbol => $"{firstType.ToDisplayString(Format.ExtendedTypeFormat)}? {argument.Key} = null",
            ITypeParameterSymbol => $"global::System.Object? {argument.Key} = null",
            _ => $"{firstType.WithNullableAnnotation(NullableAnnotation.Annotated).ToDisplayString(Format.ExtendedTypeFormat)}? {argument.Key} = null"
        };
    }
}
