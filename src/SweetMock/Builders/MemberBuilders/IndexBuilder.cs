namespace SweetMock.Builders.MemberBuilders;

using Generation;
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

        classScope.Region($"Index : this[{indexType}]", builder =>
        {
            var indexerCount = 0;
            foreach (var symbol in symbols)
            {
                indexerCount++;
                BuildIndex(builder, symbol, indexerCount);
            }
        });
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

        var signature = $"{overwrites.AccessibilityString}{overwrites.OverrideString}{returnType} {overwrites.ContainingSymbol}this[{indexType} {argName}]";
        classScope.Scope(signature, indexerScope => indexerScope
            .AddIf(hasGet, get => get
                .Scope("get", getScope => getScope
                    .BuildLogSegment(symbol.GetMethod)
                    .Add($"return this.{internalName}_get({argName});")
                ))
            .AddIf(hasSet, set => set
                .Scope("set", setScope => setScope
                    .BuildLogSegment(symbol.SetMethod)
                    .Add($"this.{internalName}_set({argName}, value);")
                )));

        classScope
            .Add($"private System.Func<{indexType}, {returnType}> {internalName}_get {{ get; set; }} = (_) => {exception}")
            .Add($"private System.Action<{indexType}, {returnType}> {internalName}_set {{ get; set; }} = (_, _) => {exception}")
            .AddLineBreak();

        classScope.AddToConfig(config =>
        {
            var indexerParameters = (hasGet ? $"System.Func<{indexType}, {returnType}> get" : "") + (hasGet && hasSet ? ", " : "") + (hasSet ? $"System.Action<{indexType}, {returnType}> set" : "");

            config.Documentation(doc => doc
                .Summary($"Configures the indexer for <see cref=\"{symbol.Parameters[0].Type.ToCRef()}\"/> by specifying methods to call when the property is accessed.")
                .Parameter("get", "Function to call when the property is read.", hasGet)
                .Parameter("set", "Function to call when the property is set.", hasSet)
                .Returns("The configuration object."));

            config.AddConfigMethod("Indexer", [indexerParameters], builder => builder
                .AddIf(hasGet, () => $"target.{internalName}_get = get;")
                .AddIf(hasSet, () => $"target.{internalName}_set = set;")
            );
        });
    }

    public static void BuildConfigExtensions(CodeBuilder codeBuilder, MockDetails mock, IEnumerable<IPropertySymbol> indexers)
    {
        foreach (var indexer in indexers)
        {
            var hasGet = indexer.GetMethod != null;
            var hasSet = indexer.SetMethod != null;

            var typeSymbol = indexer.Parameters[0].Type;
            codeBuilder.AddLineBreak();

            codeBuilder.Documentation(doc => doc
                .Summary($"Specifies a dictionary to be used as a source of the indexer for <see cref=\"{indexer.Parameters[0].Type.ToCRef()}\"/>.")
                .Parameter("values", "Dictionary containing the values for the indexer.")
                .Returns("The updated configuration object."));

            codeBuilder.AddConfigExtension(mock, indexer, [$"System.Collections.Generic.Dictionary<{typeSymbol}, {indexer.Type}> values"], builder =>
            {
                builder.AddIf(hasGet && hasSet, () => $"this.Indexer(get: ({typeSymbol} key) => values[key], set: ({typeSymbol} key, {indexer.Type} value) => values[key] = value);");
                builder.AddIf(hasGet && !hasSet, () => $"this.Indexer(get: ({typeSymbol} key) => values[key]);");
                builder.AddIf(!hasGet && hasSet, () => $"this.Indexer(set: ({typeSymbol} key, {indexer.Type} value) => values[key] = value);");
            });
        }
    }
}
