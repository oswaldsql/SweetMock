namespace SweetMock.Builders.MemberBuilders;

using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;
using Utils;

/// <summary>
///     Represents a builder for indexers, implementing the ISymbolBuilder interface.
/// </summary>
internal class IndexBuilder : IBaseClassBuilder
{
    public bool TryBuildBase(MockDetails details, CodeBuilder result, ISymbol[] symbols)
    {
        var first = symbols.First();

        if (first is not IPropertySymbol { IsIndexer: true }) return false;

        return BuildIndexes(result, symbols.OfType<IPropertySymbol>().Where(t => t.IsIndexer));
    }

    /// <summary>
    ///     Builds helper methods for the indexer.
    /// </summary>
    /// <param name="symbol">The property symbol representing the indexer.</param>
    /// <param name="indexerCount">The count of indexers built so far.</param>
    /// <returns>A collection of helper methods for the indexer.</returns>
//    private static IEnumerable<ConfigExtension> BuildHelpers(IPropertySymbol symbol, int indexerCount)
//    {
//        ConfigExtension cx(string signature, string code, string documentation, [CallerLineNumber] int ln = 0) => new(signature, code + "// line : " + ln, documentation,  symbol.ToString());
//
//        var hasGet = symbol.GetMethod != null;
//        var hasSet = symbol.SetMethod != null;
//        var returnType = symbol.Type.ToString();
//        var indexType = symbol.Parameters[0].Type.ToString();
//
//        yield return cx($"System.Collections.Generic.Dictionary<{indexType}, {returnType}> values",
//            $"""
//             target.On_IndexGet_{indexerCount} = s => values[s];
//             target.On_IndexSet_{indexerCount} = (s, v) => values[s] = v;
//             """,
//            "Gets and sets values in the dictionary when the indexer is called.");
//
//        switch (hasSet, hasGet)
//        {
//            case (true, true):
//                yield return cx($"System.Func<{indexType}, {returnType}> get, System.Action<{indexType}, {returnType}> set",
//                    $"target.On_IndexGet_{indexerCount} = get;target.On_IndexSet_{indexerCount} = set;",
//                    $"Specifies a getter and setter method to call when the indexer for <see cref=\"{indexType}\"/> is called.");
//                break;
//            case (true, false):
//                yield return cx($"System.Action<{indexType}, {returnType}> set",
//                    $"target.On_IndexSet_{indexerCount} = set;",
//                    $"Specifies a setter method to call when the indexer for <see cref=\"{indexType}\"/> is called.");
//                break;
//            case (false, true):
//                yield return cx($"System.Func<{indexType}, {returnType}> get",
//                    $"target.On_IndexGet_{indexerCount} = get;",
//                    $"Specifies a getter method to call when the indexer for <see cref=\"{indexType}\"/> is called.");
//                break;
//        }
//    }

    /// <summary>
    ///     Builds the indexers and adds them to the code builder.
    /// </summary>
    /// <param name="builder">The code builder to add the indexers to.</param>
    /// <param name="indexerSymbols">The collection of indexer symbols to build.</param>
    /// <returns>True if any indexers were built; otherwise, false.</returns>
    private static bool BuildIndexes(CodeBuilder builder, IEnumerable<IPropertySymbol> indexerSymbols)
    {
        var symbols = indexerSymbols as IPropertySymbol[] ?? indexerSymbols.ToArray();
        var indexType = symbols.First().Parameters[0].Type.ToString();

        using (builder.Region($"Index : this[{indexType}]"))
        {
            var indexerCount = 0;
            foreach (var symbol in symbols)
            {
                indexerCount++;
                BuildIndex(builder, symbol, indexerCount);
            }
            return indexerCount > 0;
        }
    }

    /// <summary>
    ///     Builds a single indexer and adds it to the code builder.
    /// </summary>
    /// <param name="builder">The code builder to add the indexer to.</param>
    /// <param name="symbol">The property symbol representing the indexer.</param>
    /// <param name="helpers">The list of helper methods to add to.</param>
    /// <param name="index">The count of indexers built so far.</param>
    private static void BuildIndex(CodeBuilder builder, IPropertySymbol symbol, int index)
    {
        var returnType = symbol.Type.ToString();
        var indexType = symbol.Parameters[0].Type.ToString();
        var exception = symbol.BuildNotMockedExceptionForIndexer();
        var internalName = index == 1 ? "_onIndex" : $"_onIndex_{index}";

        var overwrites = symbol.Overwrites();

        var hasGet = symbol.GetMethod != null;
        var hasSet = symbol.SetMethod != null;

        var argName = symbol.Parameters[0].Name;

        builder.Add($$"""{{overwrites.accessibilityString}}{{returnType}} {{overwrites.containingSymbol}}this[{{indexType}} {{argName}}] {""").Indent();

        builder.Add(hasGet, () => $$"""
                                    get {
                                        {{LogBuilder.BuildLogSegment(symbol.GetMethod)}}
                                        return this.{{internalName}}_get({{argName}});
                                    }
                                    """);
        builder.Add(hasSet, () => $$"""
                                    set {
                                        {{LogBuilder.BuildLogSegment(symbol.SetMethod)}}
                                       this.{{internalName}}_set({{argName}}, value);
                                    }
                                    """);
        builder.Unindent();

        builder.Add($$"""
                      }

                      private System.Func<{{indexType}}, {{returnType}}> {{internalName}}_get { get; set; } = (_) => {{exception}}
                      private System.Action<{{indexType}}, {{returnType}}> {{internalName}}_set { get; set; } = (_, _) => {{exception}}
                      """);

        builder.Add("internal partial class Config {");

        switch (hasSet, hasGet)
        {
            case (true, true):
                builder.Add($$"""
                                public Config Indexer(System.Func<{{indexType}}, {{returnType}}> get, System.Action<{{indexType}}, {{returnType}}> set) {
                                    target.{{internalName}}_get = get;
                                    target.{{internalName}}_set = set;
                                    return this;
                                }
                              """);
                break;
            case (false, true):
                builder.Add($$"""
                                public Config Indexer(System.Func<{{indexType}}, {{returnType}}> get) {
                                    target.{{internalName}}_get = get;
                                    return this;
                                }
                              """);
                break;
            case (true, false):
                builder.Add($$"""
                                public Config Indexer(System.Action<{{indexType}}, {{returnType}}> set) {
                                    target.{{internalName}}_set = set;
                                    return this;
                                }
                              """);
                break;
        }

        builder.Add("}");
    }
}