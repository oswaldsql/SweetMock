namespace SweetMock.Builders;

using Generation;
using Utils;

internal static class LogExtensionsBuilder
{
    internal static CodeBuilder BuildLogExtensionsClass(this CodeBuilder builder, MockContext context) =>
        builder
            .AddGeneratedCodeAttrib()
            .Scope($"internal static class {context.MockName}_LogExtensions", config =>
                BuildMembers(config, context));

    private static void BuildMembers(CodeBuilder builder, MockContext context)
    {
        var members = context.GetCandidates().Distinct(SymbolEqualityComparer.IncludeNullability).ToLookup(t => t.Name);

        var sourceName = context.Source.Name;
        builder
            .Add($"public static {sourceName}_Filter {sourceName}(this global::System.Collections.Generic.IEnumerable<CallLogItem> source) => new(source);")
            .Scope($"public class {sourceName}_Filter(global::System.Collections.Generic.IEnumerable<CallLogItem> source) : CallLogFilter(source)", filterScope => filterScope
                .Add($"protected override string SignatureStart => \"{context.Source.ToDisplayString(FullyQualifiedFormat)}.\";"));

        foreach (var m in members)
        {
            var f = m.First();
            switch (f)
            {
                case IMethodSymbol {MethodKind: MethodKind.EventAdd or MethodKind.EventRaise or MethodKind.EventRemove or MethodKind.PropertyGet or MethodKind.PropertySet}:
                    break;
                case IMethodSymbol { MethodKind: MethodKind.Constructor }:
                    BuildConstructors(builder, context, m);
                    break;
                case IMethodSymbol { MethodKind: MethodKind.Ordinary }:
                    BuildMethods(builder, context, m);
                    break;
                case IPropertySymbol { IsIndexer: false } propertySymbol:
                    BuildProperties(builder, context, m, propertySymbol);
                    break;
                case IPropertySymbol { IsIndexer: true } indexerSymbol:
                    BuildIndexer(builder, context, m, indexerSymbol);
                    break;
                case IEventSymbol eventSymbol:
                    BuildEvent(builder, context, m, eventSymbol);
                    break;
            }
        }
    }

    private static CodeBuilder BuildLoggingExtension(this CodeBuilder builder, MockContext context, IMethodSymbol? methodSymbol, ISymbol target, bool ignoreArguments = false)
    {
        if (methodSymbol == null)
        {
            return builder;
        }

        var methodName = GetMethodName(methodSymbol);
        var argsClass = $"{methodName}_Args";
        var predicateName = $"{methodSymbol.ContainingSymbol.Name}_{methodName}_Predicate";

        RenderArgumentClass(builder, methodSymbol, argsClass, ignoreArguments);

        BuildPredicateDocumentation(builder, [methodSymbol], target);

        builder
            .Add($"public static global::System.Collections.Generic.IEnumerable<{argsClass}> {methodName}(this {context.Source.Name}_Filter log, Func<{argsClass}, bool>? predicate = null) =>")
            .Indent()
            .Add($" ((ICallLogFilter)log).Filter().{methodName}(predicate);")
            .Unindent()
            .AddLineBreak();

        BuildPredicateDocumentation(builder, [methodSymbol], target);
        builder
            .Add($"public static global::System.Collections.Generic.IEnumerable<{argsClass}> {methodName}(this global::System.Collections.Generic.IEnumerable<CallLogItem> log, Func<{argsClass}, bool>? {predicateName} = null) =>")
            .Indent()
            .Add($"log.Matching<{argsClass}>(\"{MethodSignature(context, methodSymbol)}\", {predicateName});")
            .Unindent()
            .AddLineBreak();
        return builder;
    }

    private static void BuildOverwrittenLoggingExtension(this CodeBuilder builder, MockContext context, IMethodSymbol[] symbols)
    {
        if (symbols.Length == 1)
        {
            builder.BuildLoggingExtension(context, symbols[0], symbols[0]);
            return;
        }

        var methodName = GetMethodName(symbols[0]);
        var argsClass = $"{methodName}_Args";
        var predicateName = $"{symbols[0].ContainingSymbol.Name}_{methodName}_Predicate";

        builder
            .RenderArgumentClass(symbols, argsClass);

        var signatures = string.Join(", ", symbols.Select(t => $"\"{MethodSignature(context,t)}\""));

        builder.Add($"private static System.Collections.Generic.HashSet<string> {methodName}_Signatures = new System.Collections.Generic.HashSet<string> {{{signatures}}};").AddLineBreak();

        BuildPredicateDocumentation(builder, symbols, symbols[0]);

        builder.Add($"public static System.Collections.Generic.IEnumerable<{argsClass}> {methodName}(this global::System.Collections.Generic.IEnumerable<CallLogItem> log, Func<{argsClass}, bool>? {predicateName} = null) =>");
        builder.Add($"log.Matching<{argsClass}>({methodName}_Signatures, {predicateName});");
        builder.AddLineBreak();

        builder
            .Add($"public static global::System.Collections.Generic.IEnumerable<{argsClass}> {methodName}(this {context.Source.Name}_Filter log, Func<{argsClass}, bool>? predicate = null) =>")
            .Add($" ((ICallLogFilter)log).Filter().{methodName}(predicate);");
    }

    private static CodeBuilder RenderArgumentClass(this CodeBuilder builder, IMethodSymbol[] symbols, string argsClass, bool ignoreArguments = false)
    {
        var lookup = symbols.SelectMany(t => t.Parameters).ToLookup(t => t.Name, t => t.Type);
        if (lookup.Count == 0 || ignoreArguments)
        {
            return builder
                .Add($"public class {argsClass} : SweetMock.TypedArguments {{ }}")
                .AddLineBreak();
        }

        builder.Scope($"public class {argsClass} : SweetMock.TypedArguments", logScope =>
        {
            foreach (var l in lookup)
            {
                var argumentTypes = l.Distinct(SymbolEqualityComparer.Default).ToArray();
                if (argumentTypes.Length > 1)
                {
                    logScope
                        .Documentation($"Enables filtering on the {l.Key} argument.", "The argument can be different types", GenerateArgumentTypesString(argumentTypes))
                        .Add($"public object? {l.Key} => base.Arguments[\"{l.Key}\"]!;");
                }
                else if (l.First() is ITypeParameterSymbol || l.First() is INamedTypeSymbol { IsGenericType: true })
                {
                    logScope
                        .Documentation($"Enables filtering on the {l.Key} argument.", $"The argument is a generic type. ({l.First()})")
                        .Add($"public object? {l.Key} => base.Arguments[\"{l.Key}\"]!;");
                }
                else
                {
                    var p = l.First();
                    logScope
                        .Documentation($"Enables filtering on the {l.Key} argument.")
                        .Add($"public {p} {l.Key} => ({p})base.Arguments[\"{l.Key}\"]!;");
                }

                logScope.AddLineBreak();
            }
        });

        return builder;

        string GenerateArgumentTypesString(ISymbol?[] argumentTypes) => string.Join(", ", argumentTypes.Select(t => t!.ToSeeCRef()).Distinct());
    }

    private static readonly SymbolDisplayFormat MethodSignatureFormat = new(globalNamespaceStyle: SymbolDisplayGlobalNamespaceStyle.Omitted, typeQualificationStyle: SymbolDisplayTypeQualificationStyle.NameAndContainingTypes, genericsOptions: SymbolDisplayGenericsOptions.IncludeTypeConstraints, memberOptions: SymbolDisplayMemberOptions.IncludeParameters, parameterOptions: SymbolDisplayParameterOptions.IncludeType);
    private static readonly SymbolDisplayFormat FullyQualifiedFormat = SymbolDisplayFormat.FullyQualifiedFormat;

    private static string MethodSignature(MockContext context, IMethodSymbol symbol) => $"{context.Source.ToDisplayString(FullyQualifiedFormat)}.{symbol.ToDisplayString(MethodSignatureFormat)}";

    private static void BuildEvent(CodeBuilder result, MockContext context, IGrouping<string, ISymbol> m, IEventSymbol eventSymbol) =>
        result.Region("Event : " + m.Key, builder => builder
            .BuildLoggingExtension(context, eventSymbol.AddMethod, eventSymbol, true)
            .BuildLoggingExtension(context, eventSymbol.RemoveMethod, eventSymbol, true)
            .BuildLoggingExtension(context, eventSymbol.RaiseMethod, eventSymbol, true));

    private static void BuildIndexer(CodeBuilder result, MockContext context, IGrouping<string, ISymbol> m, IPropertySymbol indexerSymbol) =>
        result.Region("Indexer : " + m.First(), builder => builder
            .BuildLoggingExtension(context, indexerSymbol.GetMethod, indexerSymbol)
            .BuildLoggingExtension(context, indexerSymbol.SetMethod, indexerSymbol));

    private static void BuildProperties(CodeBuilder result, MockContext context, IGrouping<string, ISymbol> m, IPropertySymbol propertySymbol) =>
        result.Region("Property : " + m.Key, builder => builder
            .BuildLoggingExtension(context, propertySymbol.GetMethod, propertySymbol)
            .BuildLoggingExtension(context, propertySymbol.SetMethod, propertySymbol));

    private static void BuildMethods(CodeBuilder result, MockContext context, IGrouping<string, ISymbol> m) =>
        result.Region("Method : " + m.Key, builder => builder
            .BuildOverwrittenLoggingExtension(context, m.OfType<IMethodSymbol>().ToArray()));

    private static void BuildConstructors(CodeBuilder result, MockContext context, IGrouping<string, ISymbol> m) =>
        result.Region("Constructors", builder => builder
            .BuildOverwrittenLoggingExtension(context, m.OfType<IMethodSymbol>().ToArray()));

    private static string GetMethodName(IMethodSymbol methodSymbol) =>
        methodSymbol.MethodKind switch
        {
            MethodKind.Constructor => methodSymbol.ContainingType.Name,
            MethodKind.EventAdd => methodSymbol.Name.Substring(4) + "_Add",
            MethodKind.EventRaise => methodSymbol.Name.Substring(6) + "_Raise",
            MethodKind.EventRemove => methodSymbol.Name.Substring(7) + "_Remove",
            MethodKind.Ordinary => methodSymbol.Name,
            MethodKind.PropertyGet => methodSymbol.Name.Substring(4) + "_Get",
            MethodKind.PropertySet => methodSymbol.Name.Substring(4) + "_Set",
            _ => methodSymbol.Name
        };

    private static void RenderArgumentClass(CodeBuilder result, IMethodSymbol symbol, string argsClass, bool ignoreArguments = false)=>
     RenderArgumentClass(result, [symbol], argsClass, ignoreArguments);

    private static void BuildPredicateDocumentation(CodeBuilder result, IMethodSymbol[] symbols, ISymbol target) =>
        result.Documentation(GetArgumentSummery(symbols[0], target));

    private static string GetArgumentSummery(IMethodSymbol symbol, ISymbol target) =>
        symbol switch
        {
            { MethodKind : MethodKind.Constructor } => $"Identifies when the mock object for {symbol.ToSeeCRef()} {target.ToSeeCRef()} is created.",
            { MethodKind : MethodKind.EventAdd } => $"Identifies when the event {target.ToSeeCRef()} was subscribed to.",
            { MethodKind : MethodKind.EventRaise } => $"Identifies when the event {target.ToSeeCRef()} was raised.",
            { MethodKind : MethodKind.EventRemove } => $"Identifies when the event {target.ToSeeCRef()} was unsubscribed to.",
            { MethodKind : MethodKind.Ordinary } => $"Identifying calls to the method {target.ToSeeCRef()}.",
            { MethodKind : MethodKind.PropertyGet } when target is IPropertySymbol { IsIndexer : false } => $"Identifies when the property {target.ToSeeCRef()} was read.",
            { MethodKind : MethodKind.PropertySet } when target is IPropertySymbol { IsIndexer : false } => $"Identifies when the property {target.ToSeeCRef()} was set.",
            { MethodKind : MethodKind.PropertyGet } when target is IPropertySymbol { IsIndexer : true } => $"Identifies when the indexer for {symbol.Parameters[0].Type.ToSeeCRef()} was read.",
            { MethodKind : MethodKind.PropertySet } when target is IPropertySymbol { IsIndexer : true } => $"Identifies when the indexer for {symbol.Parameters[0].Type.ToSeeCRef()} was set.",
            _ => ""
        };
}
