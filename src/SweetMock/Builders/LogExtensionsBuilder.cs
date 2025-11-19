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
        var memberGroups = context
            .GetCandidates()
            .Distinct(SymbolEqualityComparer.IncludeNullability)
            .ToLookup(t => t.Name);

        var sourceName = context.Source.Name;
        builder
            .Add($"public static {sourceName}_Filter {sourceName}(this global::SweetMock.CallLog source) => new(source);")
            .Scope($"public class {sourceName}_Filter(global::SweetMock.CallLog source) : CallLogFilter(source)", filterScope => filterScope
                .Add($"protected override string SignatureStart => \"{context.Source.ToDisplayString(FullyQualifiedFormat)}.\";"));

        foreach (var members in memberGroups)
        {
            GenerateLoggingExtensions(builder, context, members);
        }
    }

    private static CodeBuilder GenerateLoggingExtensions(CodeBuilder builder, MockContext context, IGrouping<string, ISymbol> members) =>
        members.First() switch
        {
            IMethodSymbol { MethodKind: MethodKind.EventAdd or MethodKind.EventRaise or MethodKind.EventRemove or MethodKind.PropertyGet or MethodKind.PropertySet } =>
                builder,
            IMethodSymbol { MethodKind: MethodKind.Constructor } or IMethodSymbol { MethodKind: MethodKind.Ordinary } =>
                builder
                    .BuildOverwrittenLoggingExtension(context, members.OfType<IMethodSymbol>().ToArray()),
            IPropertySymbol { IsIndexer: false } propertySymbol =>
                builder
                    .BuildLoggingExtension(context, propertySymbol.GetMethod, propertySymbol)
                    .BuildLoggingExtension(context, propertySymbol.SetMethod, propertySymbol),
            IPropertySymbol { IsIndexer: true } indexerSymbol =>
                builder
                    .BuildLoggingExtension(context, indexerSymbol.GetMethod, indexerSymbol)
                    .BuildLoggingExtension(context, indexerSymbol.SetMethod, indexerSymbol),
            IEventSymbol eventSymbol =>
                builder
                    .BuildLoggingExtension(context, eventSymbol.AddMethod, eventSymbol, true)
                    .BuildLoggingExtension(context, eventSymbol.RemoveMethod, eventSymbol, true)
                    .BuildLoggingExtension(context, eventSymbol.RaiseMethod, eventSymbol, true),
            _ => builder
        };

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

        builder.BuildPredicateDocumentation([methodSymbol], target)
            .Add($"public static System.Collections.Generic.IEnumerable<{argsClass}> {methodName}(this {context.Source.Name}_Filter log, System.Func<{argsClass}, bool>? predicate = null) =>")
            .Indent()
            .Add($"log.Filter().{methodName}(predicate);")
            .Unindent()
            .BR();

        builder.BuildPredicateDocumentation([methodSymbol], target)
            .Add($"public static System.Collections.Generic.IEnumerable<{argsClass}> {methodName}(this global::SweetMock.CallLog log, System.Func<{argsClass}, bool>? {predicateName} = null) =>")
            .Indent()
            .Add($"log.Matching<{argsClass}>(\"{MethodSignature(context, methodSymbol)}\", {predicateName});")
            .Unindent()
            .BR();
        return builder;
    }

    private static CodeBuilder BuildOverwrittenLoggingExtension(this CodeBuilder builder, MockContext context, IMethodSymbol[] symbols)
    {
        if (symbols.Length == 1)
        {
            return builder.BuildLoggingExtension(context, symbols[0], symbols[0]);
        }

        var methodName = GetMethodName(symbols[0]);
        var argsClass = $"{methodName}_Args";
        var predicateName = $"{symbols[0].ContainingSymbol.Name}_{methodName}_Predicate";

        var signatures = string.Join(", ", symbols.Select(t => $"\"{MethodSignature(context,t)}\""));

        return builder
            .RenderArgumentClass(symbols, argsClass)
            .Add($"private static System.Collections.Generic.HashSet<string> {methodName}_Signatures = new System.Collections.Generic.HashSet<string> {{{signatures}}};")
            .BR()
            .BuildPredicateDocumentation(symbols, symbols[0])
            .Add($"public static System.Collections.Generic.IEnumerable<{argsClass}> {methodName}(this global::SweetMock.CallLog log, System.Func<{argsClass}, bool>? {predicateName} = null) =>")
            .Add($"log.Matching<{argsClass}>({methodName}_Signatures, {predicateName});")
            .BR()
            .Add($"public static System.Collections.Generic.IEnumerable<{argsClass}> {methodName}(this {context.Source.Name}_Filter log, System.Func<{argsClass}, bool>? predicate = null) =>")
            .Add($"    log.Filter().{methodName}(predicate);")
            ;
    }

    private static CodeBuilder RenderArgumentClass(this CodeBuilder builder, IMethodSymbol[] symbols, string argsClass, bool ignoreArguments = false)
    {
        var lookup = symbols.SelectMany(t => t.Parameters).ToLookup(t => t.Name, t => t.Type);
        if (lookup.Count == 0 || ignoreArguments)
        {
            return builder
                .Add($"public class {argsClass} : global::SweetMock.TypedArguments {{ }}")
                .BR();
        }

        builder.Scope($"public class {argsClass} : global::SweetMock.TypedArguments", logScope =>
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
                            .Documentation($"Enables filtering on the {l.Key} argument.", $"The argument is a generic type. ({l.First().ToSeeCRef()})")
                            .Add($"public object? {l.Key} => base.Arguments[\"{l.Key}\"]!;");
                    }
                    else
                    {
                        var p = l.First();
                        logScope
                            .Documentation($"Enables filtering on the {l.Key} argument.")
                            .Add($"public {p} {l.Key} => ({p})base.Arguments[\"{l.Key}\"]!;");
                    }

                    logScope.BR();
                }
            })
            .BR();

        return builder;

        string GenerateArgumentTypesString(ISymbol?[] argumentTypes) => string.Join(", ", argumentTypes.Select(t => t!.ToSeeCRef()).Distinct());
    }

    private static readonly SymbolDisplayFormat MethodSignatureFormat = new(globalNamespaceStyle: SymbolDisplayGlobalNamespaceStyle.Omitted, typeQualificationStyle: SymbolDisplayTypeQualificationStyle.NameAndContainingTypes, genericsOptions: SymbolDisplayGenericsOptions.IncludeTypeConstraints, memberOptions: SymbolDisplayMemberOptions.IncludeParameters, parameterOptions: SymbolDisplayParameterOptions.IncludeType);
    private static readonly SymbolDisplayFormat FullyQualifiedFormat = SymbolDisplayFormat.FullyQualifiedFormat;

    private static string MethodSignature(MockContext context, IMethodSymbol symbol) =>
        $"{context.Source.ToDisplayString(FullyQualifiedFormat)}.{symbol.ToDisplayString(MethodSignatureFormat)}";

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

    private static void RenderArgumentClass(CodeBuilder result, IMethodSymbol symbol, string argsClass, bool ignoreArguments = false) =>
        RenderArgumentClass(result, [symbol], argsClass, ignoreArguments);

    private static CodeBuilder BuildPredicateDocumentation(this CodeBuilder result, IMethodSymbol[] symbols, ISymbol target) =>
        result.Documentation(GetArgumentSummery(symbols[0], target));

    private static string GetArgumentSummery(IMethodSymbol symbol, ISymbol target) =>
        symbol switch
        {
            { MethodKind : MethodKind.Constructor } => $"Identifies when the mock object for {target.ToSeeCRef()} is created.",
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
