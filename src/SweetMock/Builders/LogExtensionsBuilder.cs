namespace SweetMock.Builders;

using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;
using Utils;

internal class LogExtensionsBuilder
{
    public string BuildLogExtensions(MockDetails details)
    {
        var builder = new CodeBuilder();

        builder.AddFileHeader();

        builder.Add($$"""
                      #nullable enable
                      using System.Linq;
                      using System;

                      namespace {{details.Namespace}} {
                      ->
                      [System.CodeDom.Compiler.GeneratedCode("SweetMock","{{SourceGeneratorMetadata.Version}}")]
                      internal static class {{details.MockName}}_LogExtensions {
                      ->
                      """);

        builder.Add(BuildMembers(details));

        builder.Add("""
                    <-
                    }
                    <-
                    }

                    """);

        return builder.ToString();
    }

    private CodeBuilder BuildMembers(MockDetails details)
    {
        var result = new CodeBuilder();

        var members = details.GetCandidates().Distinct(SymbolEqualityComparer.IncludeNullability).ToLookup(t => t.Name);

        foreach (var m in members)
        {
            var f = m.First();
            switch (f)
            {
                case IMethodSymbol {MethodKind: MethodKind.EventAdd or MethodKind.EventRaise or MethodKind.EventRemove or MethodKind.PropertyGet or MethodKind.PropertySet}:
                    break;
                case IMethodSymbol { MethodKind: MethodKind.Constructor }:
                    this.BuildConstructors(result, m);
                    break;
                case IMethodSymbol { MethodKind: MethodKind.Ordinary }:
                    this.BuildMethods(result, m);
                    break;
                case IPropertySymbol { IsIndexer: false } propertySymbol:
                    this.BuildProperties(result, m, propertySymbol);
                    break;
                case IPropertySymbol { IsIndexer: true } indexerSymbol:
                    this.BuildIndexer(result, m, indexerSymbol);
                    break;
                case IEventSymbol eventSymbol:
                    this.BuildEvent(result, m, eventSymbol);
                    break;
                default:
                    result.Add("//" + f);
                    break;
            }
        }

        return result;
    }

    private void BuildEvent(CodeBuilder result, IGrouping<string, ISymbol> m, IEventSymbol eventSymbol)
    {
        using (result.Region("Event : " + m.Key))
        {
            result.Add(this.BuildLoggingExtension(eventSymbol.AddMethod, eventSymbol));
            result.Add(this.BuildLoggingExtension(eventSymbol.RemoveMethod, eventSymbol));
            result.Add(this.BuildLoggingExtension(eventSymbol.RaiseMethod, eventSymbol));
        }
    }

    private void BuildIndexer(CodeBuilder result, IGrouping<string, ISymbol> m, IPropertySymbol indexerSymbol)
    {
        using (result.Region("Indexer : " + m.Key))
        {
            result.Add(this.BuildLoggingExtension(indexerSymbol.GetMethod, indexerSymbol ));
            result.Add(this.BuildLoggingExtension(indexerSymbol.SetMethod, indexerSymbol));
        }
    }

    private void BuildProperties(CodeBuilder result, IGrouping<string, ISymbol> m, IPropertySymbol propertySymbol)
    {
        using (result.Region("Property : " + m.Key))
        {
            if (propertySymbol.GetMethod != null)
            {
                result.Add(renderArgumentClass(propertySymbol.GetMethod, $"{this.GetMethodName(propertySymbol.GetMethod)}_Args"));

                //result.Add(BuildPredicateDocumentation([propertySymbol.GetMethod], propertySymbol));

                result.Add($"""
                             public static System.Collections.Generic.IEnumerable<SweetMock.TypedCallLogItem<{$"{this.GetMethodName(propertySymbol.GetMethod)}_Args"}>> {this.GetMethodName(propertySymbol.GetMethod)}(this SweetMock.CallLog log, Func<{$"{this.GetMethodName(propertySymbol.GetMethod)}_Args"}, bool>? predicate = null) =>
                                log.Matching<{$"{this.GetMethodName(propertySymbol.GetMethod)}_Args"}>("{propertySymbol.GetMethod}", predicate);

                             """);
            }

            result.Add(this.BuildLoggingExtension(propertySymbol.SetMethod,propertySymbol));
        }
    }

    private void BuildMethods(CodeBuilder result, IGrouping<string, ISymbol> m)
    {
        using (result.Region("Method : " + m.Key))
        {
            result.Add(this.BuildOverwrittenLoggingExtension(m.OfType<IMethodSymbol>().ToArray()));
        }
    }

    private void BuildConstructors(CodeBuilder result, IGrouping<string, ISymbol> m)
    {
        using (result.Region("Constructors"))
        {
            result.Add(this.BuildOverwrittenLoggingExtension(m.OfType<IMethodSymbol>().ToArray()));
        }
    }

    public CodeBuilder BuildLoggingExtension(IMethodSymbol? methodSymbol, ISymbol target)
    {
        CodeBuilder result = new();

        if (methodSymbol == null)
        {
            return result;
        }

        result.Add(renderArgumentClass(methodSymbol, $"{this.GetMethodName(methodSymbol)}_Args"));

        result.Add(BuildPredicateDocumentation([methodSymbol], target));

        result.Add($"""
                     public static System.Collections.Generic.IEnumerable<SweetMock.TypedCallLogItem<{$"{this.GetMethodName(methodSymbol)}_Args"}>> {this.GetMethodName(methodSymbol)}(this SweetMock.CallLog log, Func<{$"{this.GetMethodName(methodSymbol)}_Args"}, bool>? predicate = null) =>
                        log.Matching<{$"{this.GetMethodName(methodSymbol)}_Args"}>("{methodSymbol}", predicate);

                     """);

        return result;
    }

    public string GetMethodName(IMethodSymbol methodSymbol) =>
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

    public CodeBuilder BuildOverwrittenLoggingExtension(IMethodSymbol[] symbols)
    {
        if (symbols.Length == 1) return this.BuildLoggingExtension(symbols[0], symbols[0]);

        CodeBuilder result = new();

        var propertyName = GetMethodName(symbols[0]);

        var argsClass = $"{propertyName}_Args";

        result.Add(renderArgumentClass(symbols, argsClass));

        var signatures = string.Join(", ", symbols.Select(t => $"\"{t}\""));

        result.Add($"private static System.Collections.Generic.HashSet<string> {propertyName}_Signatures = new System.Collections.Generic.HashSet<string> {{{signatures}}};").Add();

        result.Add(BuildPredicateDocumentation(symbols, symbols[0]));

        result.Add($$"""
                     public static System.Collections.Generic.IEnumerable<SweetMock.TypedCallLogItem<{{argsClass}}>> {{propertyName}}(this SweetMock.CallLog log, Func<{{argsClass}}, bool>? predicate = null) =>
                        log.Matching<{{argsClass}}>({{propertyName}}_Signatures, predicate);

                     """);

        return result;
    }

    private static CodeBuilder renderArgumentClass(IMethodSymbol symbol, string argsClass)=>
     renderArgumentClass([symbol], argsClass);

    private static CodeBuilder renderArgumentClass(IMethodSymbol[] symbols, string argsClass)
    {
        CodeBuilder result = new();

        var lookup = symbols.OfType<IMethodSymbol>().SelectMany(t => t.Parameters).ToLookup(t => t.Name, t => t.Type);
        if(lookup.Count == 0)
            return result.Add($"public class {argsClass} : SweetMock.TypedArguments {{ }}").Add();

        result.Add($"public class {argsClass} : SweetMock.TypedArguments {{").Indent();
        foreach (var l in lookup)
            if (l.Count() > 1)
            {
                result.AddSummary("");
                result.Add($"public object? {l.Key} => base.Arguments[\"{l.Key}\"]!;");
            }
            else if(l.First() is ITypeParameterSymbol)
            {
                result.AddSummary("The argument is a generic type.")
                    .Add($"public object? {l.Key} => base.Arguments[\"{l.Key}\"]!;");
            }
            else
            {
                var p = l.First();
                result.Add($"public {p} {l.Key} => ({p})base.Arguments[\"{l.Key}\"]!;");
            }

        result.Unindent().Add("}");

        return result;
    }

    private static CodeBuilder BuildPredicateDocumentation(IMethodSymbol[] symbols, ISymbol target)
    {
        CodeBuilder result = new();
        result.Add("/// <summary>");
        result.Add("///     "+ GetArgumentSummery(symbols[0], target));
        result.Add($"/// <see cref=\"{target.ToCRef()}\"/>");
        foreach (var symbol in symbols)
        {
            result.Add($"/// <see cref=\"{symbol.ToCRef()}\"/>");
        }
        result.Add("/// </summary>");
        return result;
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
