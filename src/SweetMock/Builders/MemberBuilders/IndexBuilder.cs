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
    public static void Build(CodeBuilder classScope, IEnumerable<IPropertySymbol> symbols)
    {
        var lookup = symbols.ToLookup(t => t.Name);
        foreach (var m in lookup)
        {
            BuildIndexes(classScope, m.ToArray());
        }
    }

    /// <summary>
    ///     Builds the indexers and adds them to the code builder.
    /// </summary>
    /// <param name="classScope"></param>
    /// <param name="indexerSymbols">The collection of indexer symbols to build.</param>
    /// <returns>True if any indexers were built; otherwise, false.</returns>
    private static void BuildIndexes(CodeBuilder classScope, IEnumerable<IPropertySymbol> indexerSymbols)
    {
        var symbols = indexerSymbols as IPropertySymbol[] ?? indexerSymbols.ToArray();
        var indexType = symbols.First().Parameters[0].Type.ToString();

        using (classScope.Region($"Index : this[{indexType}]"))
        {
            var indexerCount = 0;
            foreach (var symbol in symbols)
            {
                indexerCount++;
                BuildIndex(classScope, symbol, indexerCount);
            }
        }
    }

    /// <summary>
    ///     Builds a single indexer and adds it to the code builder.
    /// </summary>
    /// <param name="classScope"></param>
    /// <param name="symbol">The property symbol representing the indexer.</param>
    /// <param name="index">The count of indexers built so far.</param>
    private static void BuildIndex(CodeBuilder classScope, IPropertySymbol symbol, int index)
    {
        var returnType = symbol.Type.ToString();
        var indexType = symbol.Parameters[0].Type.ToString();
        var exception = symbol.BuildNotMockedExceptionForIndexer();
        var internalName = index == 1 ? "_onIndex" : $"_onIndex_{index}";

        var overwrites = symbol.Overwrites();

        var hasGet = symbol.GetMethod != null;
        var hasSet = symbol.SetMethod != null;

        var argName = symbol.Parameters[0].Name;

        classScope.AddLines($$"""{{overwrites.AccessibilityString}}{{overwrites.OverrideString}}{{returnType}} {{overwrites.ContainingSymbol}}this[{{indexType}} {{argName}}] {""").Indent();

        classScope.Condition(hasGet, b => b.AddLines("get {").Indent()
            .BuildLogSegment(symbol.GetMethod)
            .AddLines($"return this.{internalName}_get({argName});")
            .Unindent().AddLines("}"));

        classScope.Condition(hasSet, b => b.AddLines("set {").Indent()
            .BuildLogSegment(symbol.SetMethod)
            .AddLines($"this.{internalName}_set({argName}, value);")
            .Unindent().AddLines("}"));

        classScope.Unindent().AddLines("}");

        classScope.AddLines($$"""
                         private System.Func<{{indexType}}, {{returnType}}> {{internalName}}_get { get; set; } = (_) => {{exception}}
                         private System.Action<{{indexType}}, {{returnType}}> {{internalName}}_set { get; set; } = (_, _) => {{exception}}
                         """);

        using (classScope.AddToConfig())
        {
            var p = (hasGet ? $"System.Func<{indexType}, {returnType}> get" : "") + (hasGet && hasSet ? ", ":"") + (hasSet ? $"System.Action<{indexType}, {returnType}> set" : "");

            classScope.AddSummary($"Configures the indexer for <see cref=\"{symbol.Parameters[0].Type.ToCRef()}\"/> by specifying methods to call when the property is accessed.")
                .AddParameter("get", "Function to call when the property is read.", hasGet)
                .AddParameter("set", "Function to call when the property is set.", hasGet)
                .AddReturns("The configuration object.")
                .AddLines($$"""public Config Indexer({{p}}) {""").Indent()
                .Add(hasGet, () => $"target.{internalName}_get = get;")
                .Add(hasSet, () => $"target.{internalName}_set = set;")
                .AddLines("return this;")
                .Unindent().AddLines("}");
        }
    }

    public static void BuildConfigExtensions(CodeBuilder codeBuilder, MockDetails mock, IEnumerable<IPropertySymbol> indexers)
    {
        foreach (var indexer in indexers)
        {
            var hasGet = indexer.GetMethod != null;
            var hasSet = indexer.SetMethod != null;

            var typeSymbol = indexer.Parameters[0].Type;
            codeBuilder.AddLineBreak();
            codeBuilder.AddSummary("Gets or sets values in the dictionary when the indexer is called.", $"Configures <see cref=\"{indexer.ToCRef()}\" />");
            codeBuilder.AddParameter("values", "Dictionary containing the values for the indexer.");
            codeBuilder.AddReturns("The updated configuration object.");
            codeBuilder.AddConfigExtension(mock, indexer, [$"System.Collections.Generic.Dictionary<{typeSymbol}, {indexer.Type}> values"], builder =>
            {
                builder.Add(hasGet && hasSet, () => $"this.Indexer(get: ({typeSymbol} key) => values[key], set: ({typeSymbol} key, {indexer.Type} value) => values[key] = value);");
                builder.Add(hasGet && !hasSet, () => $"this.Indexer(get: ({typeSymbol} key) => values[key]);");
                builder.Add(!hasGet && hasSet, () => $"this.Indexer(set: ({typeSymbol} key, {indexer.Type} value) => values[key] = value);");
            });
        }
    }
}
