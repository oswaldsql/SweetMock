namespace SweetMock.Builders;

using System.Linq;
using Generation;
using Microsoft.CodeAnalysis;
using Utils;

internal static class LogExtensionsBuilder
{
    public static string BuildLogExtensions(MockDetails details)
    {
        var builder = new CodeBuilder();

        builder.AddFileHeader();

        builder
            .Add("#nullable enable")
            .Add("using System.Linq;")
            .Add("using System;")
            .AddLineBreak();

        builder.Scope($"namespace {details.Namespace}", namespaceScope => namespaceScope
            .AddGeneratedCodeAttrib()
            .Scope($"internal static class {details.MockName}_LogExtensions", config =>
                BuildMembers(config, details)));

        return builder.ToString();
    }

    private static void BuildMembers(CodeBuilder builder, MockDetails details)
    {
        var members = details.GetCandidates().Distinct(SymbolEqualityComparer.IncludeNullability).ToLookup(t => t.Name);

        foreach (var m in members)
        {
            var f = m.First();
            switch (f)
            {
                case IMethodSymbol {MethodKind: MethodKind.EventAdd or MethodKind.EventRaise or MethodKind.EventRemove or MethodKind.PropertyGet or MethodKind.PropertySet}:
                    break;
                case IMethodSymbol { MethodKind: MethodKind.Constructor }:
                    BuildConstructors(builder, m);
                    break;
                case IMethodSymbol { MethodKind: MethodKind.Ordinary }:
                    BuildMethods(builder, m);
                    break;
                case IPropertySymbol { IsIndexer: false } propertySymbol:
                    BuildProperties(builder, m, propertySymbol);
                    break;
                case IPropertySymbol { IsIndexer: true } indexerSymbol:
                    BuildIndexer(builder, m, indexerSymbol);
                    break;
                case IEventSymbol eventSymbol:
                    BuildEvent(builder, m, eventSymbol);
                    break;
            }
        }
    }

    private static void BuildEvent(CodeBuilder result, IGrouping<string, ISymbol> m, IEventSymbol eventSymbol) =>
        result.Region("Event : " + m.Key, builder => builder
            .BuildLoggingExtension(eventSymbol.AddMethod, eventSymbol, true)
            .BuildLoggingExtension(eventSymbol.RemoveMethod, eventSymbol, true)
            .BuildLoggingExtension(eventSymbol.RaiseMethod, eventSymbol, true));

    private static void BuildIndexer(CodeBuilder result, IGrouping<string, ISymbol> m, IPropertySymbol indexerSymbol) =>
        result.Region("Indexer : " + m.First(), builder => builder
            .BuildLoggingExtension(indexerSymbol.GetMethod, indexerSymbol)
            .BuildLoggingExtension(indexerSymbol.SetMethod, indexerSymbol));

    private static void BuildProperties(CodeBuilder result, IGrouping<string, ISymbol> m, IPropertySymbol propertySymbol) =>
        result.Region("Property : " + m.Key, builder => builder
            .BuildLoggingExtension(propertySymbol.GetMethod, propertySymbol)
            .BuildLoggingExtension(propertySymbol.SetMethod, propertySymbol));

    private static void BuildMethods(CodeBuilder result, IGrouping<string, ISymbol> m) =>
        result.Region("Method : " + m.Key, builder => builder
            .BuildOverwrittenLoggingExtension(m.OfType<IMethodSymbol>().ToArray()));

    private static void BuildConstructors(CodeBuilder result, IGrouping<string, ISymbol> m) =>
        result.Region("Constructors", builder => builder
            .BuildOverwrittenLoggingExtension(m.OfType<IMethodSymbol>().ToArray()));

    private static CodeBuilder BuildLoggingExtension(this CodeBuilder result, IMethodSymbol? methodSymbol, ISymbol target, bool ignoreArguments = false)
    {
        if (methodSymbol == null)
        {
            return result;
        }

        RenderArgumentClass(result, methodSymbol, $"{GetMethodName(methodSymbol)}_Args", ignoreArguments);

        BuildPredicateDocumentation(result, [methodSymbol], target);

        return result.AddLines($"""
                    public static System.Collections.Generic.IEnumerable<SweetMock.TypedCallLogItem<{$"{GetMethodName(methodSymbol)}_Args"}>> {GetMethodName(methodSymbol)}(this SweetMock.CallLog log, Func<{$"{GetMethodName(methodSymbol)}_Args"}, bool>? predicate = null) =>
                       log.Matching<{$"{GetMethodName(methodSymbol)}_Args"}>("{methodSymbol}", predicate);

                    """);
    }

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

    private static void BuildOverwrittenLoggingExtension(this CodeBuilder result, IMethodSymbol[] symbols)
    {
        if (symbols.Length == 1)
        {
            result.BuildLoggingExtension(symbols[0], symbols[0]);
            return;
        }

        var propertyName = GetMethodName(symbols[0]);

        var argsClass = $"{propertyName}_Args";

        RenderArgumentClass(result, symbols, argsClass);

        var signatures = string.Join(", ", symbols.Select(t => $"\"{t}\""));

        result.Add($"private static System.Collections.Generic.HashSet<string> {propertyName}_Signatures = new System.Collections.Generic.HashSet<string> {{{signatures}}};").AddLineBreak();

        BuildPredicateDocumentation(result, symbols, symbols[0]);

        result.AddLines($$"""
                     public static System.Collections.Generic.IEnumerable<SweetMock.TypedCallLogItem<{{argsClass}}>> {{propertyName}}(this SweetMock.CallLog log, Func<{{argsClass}}, bool>? predicate = null) =>
                        log.Matching<{{argsClass}}>({{propertyName}}_Signatures, predicate);

                     """);
    }

    private static void RenderArgumentClass(CodeBuilder result, IMethodSymbol symbol, string argsClass, bool ignoreArguments = false)=>
     RenderArgumentClass(result, [symbol], argsClass, ignoreArguments);

    private static void RenderArgumentClass(CodeBuilder result, IMethodSymbol[] symbols, string argsClass, bool ignoreArguments = false)
    {
        var lookup = symbols.SelectMany(t => t.Parameters).ToLookup(t => t.Name, t => t.Type);
        if (lookup.Count == 0 || ignoreArguments)
        {
            result.Add($"public class {argsClass} : SweetMock.TypedArguments {{ }}").AddLineBreak();
            return;
        }

        result.Add($"public class {argsClass} : SweetMock.TypedArguments {{").Indent();
        if (!ignoreArguments)
        {
            foreach (var l in lookup)
            {
                if (l.Count() > 1)
                {
                    result.Documentation(doc => doc
                        .Summary("The argument can be different types", string.Join(", ", l.Select(t => t.ToCRef()))));

                    result.Add($"public object? {l.Key} => base.Arguments[\"{l.Key}\"]!;");
                }
                else if (l.First() is ITypeParameterSymbol || l.First() is INamedTypeSymbol { IsGenericType: true })
                {
                    result.Documentation(doc => doc
                        .Summary("The argument is a generic type. (" + l.First() + ")"));

                    result.Add($"public object? {l.Key} => base.Arguments[\"{l.Key}\"]!;");
                }
                else
                {
                    var p = l.First();
                    result.Add($"public {p} {l.Key} => ({p})base.Arguments[\"{l.Key}\"]!;");
                }
            }
        }

        result.Unindent().Add("}");
    }

    private static void BuildPredicateDocumentation(CodeBuilder result, IMethodSymbol[] symbols, ISymbol target) =>
        result.Documentation(doc => doc
            .Summary(GetArgumentSummery(symbols[0], target)));

    private static string GetArgumentSummery(IMethodSymbol symbol, ISymbol target) =>
        symbol switch
        {
            { MethodKind : MethodKind.Constructor } => $"Identifies when the mock object for <see cref=\"{symbol.ToCRef()}\"/> <see cref=\"{target.ToCRef()}\"/> is created.",
            { MethodKind : MethodKind.EventAdd } => $"Identifies when the event <see cref=\"{target.ToCRef()}\"/> was subscribed to.",
            { MethodKind : MethodKind.EventRaise } => $"Identifies when the event <see cref=\"{target.ToCRef()}\"/> was raised.",
            { MethodKind : MethodKind.EventRemove } => $"Identifies when the event <see cref=\"{target.ToCRef()}\"/> was unsubscribed to.",
            { MethodKind : MethodKind.Ordinary } => $"Identifying calls to the method <see cref=\"{target.ToCRef()}\"/>.",
            { MethodKind : MethodKind.PropertyGet } when target is IPropertySymbol { IsIndexer : false } => $"Identifies when the property <see cref=\"{target.ToCRef()}\"/> was read.",
            { MethodKind : MethodKind.PropertySet } when target is IPropertySymbol { IsIndexer : false } => $"Identifies when the property <see cref=\"{target.ToCRef()}\"/> was set.",
            { MethodKind : MethodKind.PropertyGet } when target is IPropertySymbol { IsIndexer : true } => $"Identifies when the indexer for <see cref=\"{symbol.Parameters[0].Type.ToCRef()}\"/> was read.",
            { MethodKind : MethodKind.PropertySet } when target is IPropertySymbol { IsIndexer : true } => $"Identifies when the indexer for <see cref=\"{symbol.Parameters[0].Type.ToCRef()}\"/> was set.",
            _ => ""
        };
}
