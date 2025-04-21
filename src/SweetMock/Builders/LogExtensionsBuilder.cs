namespace SweetMock.Builders;

using System.Linq;
using Microsoft.CodeAnalysis;
using Utils;

internal static class LogExtensionsBuilder
{
    public static string BuildLogExtensions(MockDetails details)
    {
        var builder = new CodeBuilder();

        builder.AddFileHeader();

        builder.AddLines($$"""
                      #nullable enable
                      using System.Linq;
                      using System;

                      namespace {{details.Namespace}} {
                      ->
                      [System.CodeDom.Compiler.GeneratedCode("SweetMock","{{SourceGeneratorMetadata.Version}}")]
                      internal static class {{details.MockName}}_LogExtensions {
                      ->
                      """);

        BuildMembers(builder, details);

        builder.AddLines("""
                    <-
                    }
                    <-
                    }

                    """);

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
                default:
                    builder.AddLines("//" + f);
                    break;
            }
        }
    }

    private static void BuildEvent(CodeBuilder result, IGrouping<string, ISymbol> m, IEventSymbol eventSymbol)
    {
        using (result.Region("Event : " + m.Key))
        {
            BuildLoggingExtension(result, eventSymbol.AddMethod, eventSymbol, true);
            BuildLoggingExtension(result, eventSymbol.RemoveMethod, eventSymbol, true);
            BuildLoggingExtension(result, eventSymbol.RaiseMethod, eventSymbol, true);
        }
    }

    private static void BuildIndexer(CodeBuilder result, IGrouping<string, ISymbol> m, IPropertySymbol indexerSymbol)
    {
        using (result.Region("Indexer : " + m.First()))
        {
            BuildLoggingExtension(result, indexerSymbol.GetMethod, indexerSymbol );
            BuildLoggingExtension(result, indexerSymbol.SetMethod, indexerSymbol);
        }
    }

    private static void BuildProperties(CodeBuilder result, IGrouping<string, ISymbol> m, IPropertySymbol propertySymbol)
    {
        using (result.Region("Property : " + m.Key))
        {
            if (propertySymbol.GetMethod != null)
            {
                RenderArgumentClass(result, propertySymbol.GetMethod, $"{GetMethodName(propertySymbol.GetMethod)}_Args");

                //result.Add(BuildPredicateDocumentation([propertySymbol.GetMethod], propertySymbol));

                result.AddLines($"""
                             public static System.Collections.Generic.IEnumerable<SweetMock.TypedCallLogItem<{$"{GetMethodName(propertySymbol.GetMethod)}_Args"}>> {GetMethodName(propertySymbol.GetMethod)}(this SweetMock.CallLog log, Func<{$"{GetMethodName(propertySymbol.GetMethod)}_Args"}, bool>? predicate = null) =>
                                log.Matching<{$"{GetMethodName(propertySymbol.GetMethod)}_Args"}>("{propertySymbol.GetMethod}", predicate);

                             """);
            }

            BuildLoggingExtension(result, propertySymbol.SetMethod,propertySymbol);
        }
    }

    private static void BuildMethods(CodeBuilder result, IGrouping<string, ISymbol> m)
    {
        using (result.Region("Method : " + m.Key))
        {
            BuildOverwrittenLoggingExtension(result, m.OfType<IMethodSymbol>().ToArray());
        }
    }

    private static void BuildConstructors(CodeBuilder result, IGrouping<string, ISymbol> m)
    {
        using (result.Region("Constructors"))
        {
            BuildOverwrittenLoggingExtension(result, m.OfType<IMethodSymbol>().ToArray());
        }
    }

    private static void BuildLoggingExtension(CodeBuilder result, IMethodSymbol? methodSymbol, ISymbol target, bool ignoreArguments = false)
    {
        if (methodSymbol == null)
        {
            return;
        }

        RenderArgumentClass(result, methodSymbol, $"{GetMethodName(methodSymbol)}_Args", ignoreArguments);

        BuildPredicateDocumentation(result, [methodSymbol], target);

        result.AddLines($"""
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

    private static void BuildOverwrittenLoggingExtension(CodeBuilder result, IMethodSymbol[] symbols)
    {
        if (symbols.Length == 1)
        {
            BuildLoggingExtension(result, symbols[0], symbols[0]);
            return;
        }

        var propertyName = GetMethodName(symbols[0]);

        var argsClass = $"{propertyName}_Args";

        RenderArgumentClass(result, symbols, argsClass);

        var signatures = string.Join(", ", symbols.Select(t => $"\"{t}\""));

        result.AddLines($"private static System.Collections.Generic.HashSet<string> {propertyName}_Signatures = new System.Collections.Generic.HashSet<string> {{{signatures}}};").AddLineBreak();

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
            result.AddLines($"public class {argsClass} : SweetMock.TypedArguments {{ }}").AddLineBreak();
            return;
        }

        result.AddLines($"public class {argsClass} : SweetMock.TypedArguments {{").Indent();
        if (!ignoreArguments)
        {
            foreach (var l in lookup)
            {
                if (l.Count() > 1)
                {
                    result.AddSummary("");
                    result.AddLines($"public object? {l.Key} => base.Arguments[\"{l.Key}\"]!;");
                }
                else if (l.First() is ITypeParameterSymbol)
                {
                    result.AddSummary("The argument is a generic type.")
                        .AddLines($"public object? {l.Key} => base.Arguments[\"{l.Key}\"]!;");
                }
                else
                {
                    var p = l.First();
                    result.AddLines($"public {p} {l.Key} => ({p})base.Arguments[\"{l.Key}\"]!;");
                }
            }
        }

        result.Unindent().AddLines("}");
    }

    private static void BuildPredicateDocumentation(CodeBuilder result, IMethodSymbol[] symbols, ISymbol target)
    {
        result.AddLines("/// <summary>");
        result.AddLines("///     "+ GetArgumentSummery(symbols[0], target));
        result.AddLines($"/// <see cref=\"{target.ToCRef()}\"/>");
        foreach (var symbol in symbols)
        {
            result.AddLines($"/// <see cref=\"{symbol.ToCRef()}\"/>");
        }
        result.AddLines("/// </summary>");
    }

    private static string GetArgumentSummery(IMethodSymbol symbol, ISymbol target) =>
        symbol switch
        {
            { MethodKind : MethodKind.Constructor } => $"Identifies when the mock object was constructed {symbol.Name}.",
            { MethodKind : MethodKind.EventAdd } => $"Identifies when the event {target.Name} was subscribed to.",
            { MethodKind : MethodKind.EventRaise } => $"Identifies when the event {target.Name} was raised.",
            { MethodKind : MethodKind.EventRemove } => $"Identifies when the event {target.Name} was unsubscribed to.",
            { MethodKind : MethodKind.Ordinary } => $"Identifying calls to the method {target.Name}.",
            { MethodKind : MethodKind.PropertyGet } when target is IPropertySymbol { IsIndexer : false } => $"Identifies when the property {target.Name} was read.",
            { MethodKind : MethodKind.PropertySet } when target is IPropertySymbol { IsIndexer : false } => $"Identifies when the property {target.Name} was set.",
            { MethodKind : MethodKind.PropertyGet } when target is IPropertySymbol { IsIndexer : true } => $"Identifies when the indexer for {symbol.Parameters[0].Type} was read.",
            { MethodKind : MethodKind.PropertySet } when target is IPropertySymbol { IsIndexer : true } => $"Identifies when the indexer for {symbol.Parameters[0].Type} was set.",
            _ => ""
        };
}
