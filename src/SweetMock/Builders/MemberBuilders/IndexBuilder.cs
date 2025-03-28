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
    /// <param name="builder">The code builder to add the indexer to.</param>
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

        return builder;
    }
}
