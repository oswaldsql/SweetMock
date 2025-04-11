namespace SweetMock.Builders.MemberBuilders;

using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;
using Utils;

/// <summary>
///     Represents a builder for indexers, implementing the ISymbolBuilder interface.
/// </summary>
internal static class IndexBuilder
{
    public static CodeBuilder Build(IEnumerable<IPropertySymbol> symbols)
    {
        CodeBuilder result = new();

        var lookup = symbols.ToLookup(t => t.Name);
        foreach (var m in lookup)
        {
            result.Add(BuildIndexes(m.ToArray()));
        }

        return result;
    }

    /// <summary>
    ///     Builds the indexers and adds them to the code builder.
    /// </summary>
    /// <param name="indexerSymbols">The collection of indexer symbols to build.</param>
    /// <returns>True if any indexers were built; otherwise, false.</returns>
    private static CodeBuilder BuildIndexes(IEnumerable<IPropertySymbol> indexerSymbols)
    {
        CodeBuilder builder = new();

        var symbols = indexerSymbols as IPropertySymbol[] ?? indexerSymbols.ToArray();
        var indexType = symbols.First().Parameters[0].Type.ToString();

        using (builder.Region($"Index : this[{indexType}]"))
        {
            var indexerCount = 0;
            foreach (var symbol in symbols)
            {
                indexerCount++;
                builder.Add(BuildIndex(symbol, indexerCount));
            }
        }

        return builder;
    }

    /// <summary>
    ///     Builds a single indexer and adds it to the code builder.
    /// </summary>
    /// <param name="symbol">The property symbol representing the indexer.</param>
    /// <param name="index">The count of indexers built so far.</param>
    private static CodeBuilder BuildIndex(IPropertySymbol symbol, int index)
    {
        CodeBuilder builder = new();

        var returnType = symbol.Type.ToString();
        var indexType = symbol.Parameters[0].Type.ToString();
        var exception = symbol.BuildNotMockedExceptionForIndexer();
        var internalName = index == 1 ? "_onIndex" : $"_onIndex_{index}";

        var overwrites = symbol.Overwrites();

        var hasGet = symbol.GetMethod != null;
        var hasSet = symbol.SetMethod != null;

        var argName = symbol.Parameters[0].Name;

        builder.Add($$"""{{overwrites.AccessibilityString}}{{returnType}} {{overwrites.ContainingSymbol}}this[{{indexType}} {{argName}}] {""").Indent();

        builder.Condition(hasGet, b => b.Add("get {").Indent()
            .BuildLogSegment(symbol.GetMethod)
            .Add($"return this.{internalName}_get({argName});")
            .Unindent().Add("}"));

        builder.Condition(hasSet, b => b.Add("set {").Indent()
            .BuildLogSegment(symbol.SetMethod)
            .Add($"this.{internalName}_set({argName}, value);")
            .Unindent().Add("}"));

        builder.Unindent().Add("}");

        builder.Add($$"""
                      private System.Func<{{indexType}}, {{returnType}}> {{internalName}}_get { get; set; } = (_) => {{exception}}
                      private System.Action<{{indexType}}, {{returnType}}> {{internalName}}_set { get; set; } = (_, _) => {{exception}}
                      """);

        using (builder.AddToConfig())
        {
            var p = (hasGet ? $"System.Func<{indexType}, {returnType}> get" : "") + (hasGet && hasSet ? ", ":"") + (hasSet ? $"System.Action<{indexType}, {returnType}> set" : "");

            builder.AddSummary($"Configures the indexer for <see cref=\"{symbol.Parameters[0].Type.ToCRef()}\"/> by specifying methods to call when the property is accessed.")
                .AddParameter("get", "Function to call when the property is read.", hasGet)
                .AddParameter("set", "Function to call when the property is set.", hasGet)
                .AddReturns("The configuration object.")
                .Add($$"""public Config Indexer({{p}}) {""").Indent()
                .Add(hasGet, () => $"target.{internalName}_get = get;")
                .Add(hasSet, () => $"target.{internalName}_set = set;")
                .Add("return this;")
                .Unindent().Add("}");
        }

        return builder;
    }

    public static CodeBuilder BuildConfigExtensions(MockDetails mock, IEnumerable<IPropertySymbol> indexers)
    {
        var result = new CodeBuilder();

        foreach (var indexer in indexers)
        {
            var hasGet = indexer.GetMethod != null;
            var hasSet = indexer.SetMethod != null;

            var typeSymbol = indexer.Parameters[0].Type;
            result.Add();
            result.AddSummary("Gets or sets values in the dictionary when the indexer is called.", $"Configures <see cref=\"{indexer.ToCRef()}\" />");
            result.AddParameter("config", "The configuration object used to set up the mock.");
            result.AddParameter("values", "Dictionary containing the values for the indexer.");
            result.AddReturns("The updated configuration object.");
            result.AddConfigExtension(mock, indexer, [$"System.Collections.Generic.Dictionary<{typeSymbol}, {indexer.Type}> values"], builder =>
            {
                builder.Add(hasGet && hasSet, () => $"config.Indexer(get: ({typeSymbol} key) => values[key], set: ({typeSymbol} key, {indexer.Type} value) => values[key] = value);");
                builder.Add(hasGet && !hasSet, () => $"config.Indexer(get: ({typeSymbol} key) => values[key]);");
                builder.Add(!hasGet && hasSet, () => $"config.Indexer(set: ({typeSymbol} key, {indexer.Type} value) => values[key] = value);");
            });
        }

        return result;
    }
}
