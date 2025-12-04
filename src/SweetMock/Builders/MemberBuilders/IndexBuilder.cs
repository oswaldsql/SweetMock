namespace SweetMock.Builders.MemberBuilders;

using Generation;
using Utils;

/// <summary>
///     Represents a builder for indexers, implementing the ISymbolBuilder interface.
/// </summary>
internal class IndexBuilder(MockContext context)
{
    public static void Render(CodeBuilder classScope, MockContext context, IEnumerable<IPropertySymbol> symbols)
    {
        var builder = new IndexBuilder(context);
        builder.Build(classScope, symbols);
    }

    private void Build(CodeBuilder classScope, IEnumerable<IPropertySymbol> symbols)
    {
        var lookup = symbols.ToArray();

        if (lookup.Length == 0)
        {
            return;
        }

        this.CreateLogArgumentsRecord(classScope, lookup);
        this.BuildIndexes(classScope, lookup);
    }

    private void CreateLogArgumentsRecord(CodeBuilder classScope, IPropertySymbol[] lookup) =>
        classScope
            .Add("public record Indexer_Arguments(")
            .Indent(scope => scope
                .Add("global::System.String? InstanceName,")
                .Add("global::System.String MethodSignature,")
                .Add($"{lookup.GenerateKeyType()} key = null,")
                .Add($"{lookup.GenerateReturnType()} value = null")
            )
            .Add(") : ArgumentBase(_containerName, \"Indexer\", MethodSignature, InstanceName);")
            .BR();

    /// <summary>
    ///     Builds the indexers and adds them to the code builder.
    /// </summary>
    /// <param name="classScope"></param>
    /// <param name="indexerSymbols">The collection of indexer symbols to build.</param>
    /// <returns>True if any indexers were built; otherwise, false.</returns>
    private void BuildIndexes(CodeBuilder classScope, IEnumerable<IPropertySymbol> indexerSymbols)
    {
        var symbols = indexerSymbols as IPropertySymbol[] ?? indexerSymbols.ToArray();

        var indexerCount = 1;
        foreach (var symbol in symbols)
        {
            var indexType = symbol.Parameters[0].Type.ToString();
            classScope.Region($"Index : this[{indexType}]", builder =>
            {
                this.BuildIndex(builder, symbol, indexerCount);
                this.BuildConfigExtensions(classScope, symbol, indexerCount);

                indexerCount++;
            });
        }
    }

    /// <summary>
    ///     Builds a single indexer and adds it to the code builder.
    /// </summary>
    /// <param name="classScope"></param>
    /// <param name="symbol">The property symbol representing the indexer.</param>
    /// <param name="index">The count of indexers built so far.</param>
    private void BuildIndex(CodeBuilder classScope, IPropertySymbol symbol, int index)
    {
        var returnType = symbol.Type.ToString();
        var indexType = symbol.Parameters[0].Type.ToString();
        var internalName = index == 1 ? "_onIndex" : $"_onIndex_{index}";

        var overwrites = symbol.Overwrites();

        var hasGet = symbol.GetMethod != null;
        var hasSet = symbol.SetMethod != null;

        var argName = symbol.Parameters[0].Name;

        var signature = $"{overwrites.AccessibilityString}{overwrites.OverrideString}{returnType} {overwrites.ContainingSymbol}this[{indexType} {argName}]";
        classScope.Scope(signature, indexerScope => indexerScope
            .AddIf(hasGet, get => get
                .Scope("get", getScope => getScope
                    .Add($"this._log(new Indexer_Arguments(this._sweetMockInstanceName, \"get\", key : {symbol.GetMethod!.Parameters[0].Name}));")
                    .Scope($"if (this.{internalName}_get is null)", ifScope => ifScope
                        .Add($"throw new global::SweetMock.NotExplicitlyMockedException(\"{symbol.Name}\", this._sweetMockInstanceName);"))
                    .Add($"return this.{internalName}_get({argName});")
                ))
            .AddIf(hasSet, set => set
                .Scope("set", setScope => setScope
                    .Add($"this._log(new Indexer_Arguments(this._sweetMockInstanceName, \"set\", key : {symbol.SetMethod!.Parameters[0].Name}, value : {symbol.SetMethod.Parameters[1].Name}));")
                    .Scope($"if (this.{internalName}_set is null)", ifScope => ifScope
                        .Add($"throw new global::SweetMock.NotExplicitlyMockedException(\"{symbol.Name}\", this._sweetMockInstanceName);"))
                    .Add($"this.{internalName}_set({argName}, value);")
                )));

        classScope
            .BR()
            .AddIf(hasGet, () => $"private System.Func<{indexType}, {returnType}>? {internalName}_get {{ get; set; }} = null;")
            .AddIf(hasSet, () => $"private System.Action<{indexType}, {returnType}>? {internalName}_set {{ get; set; }} = null;")
            .BR();
    }

    private void BuildConfigExtensions(CodeBuilder classScope, IPropertySymbol symbol, int index)
    {
        var internalName = index == 1 ? "_onIndex" : $"_onIndex_{index}";
        classScope.AddToConfig(context, config =>
        {
            this.AddGetSetConfiguration(symbol, config, internalName);
            this.GenerateIndexerConfigExtensions(config, symbol);
        });
    }

    private void AddGetSetConfiguration(IPropertySymbol symbol, CodeBuilder config, string internalName)
    {
        var hasGet = symbol.GetMethod != null;
        var hasSet = symbol.SetMethod != null;

        var typeSymbol = symbol.Parameters[0].Type;
        var returnType = symbol.Type.ToString();

        var indexerParameters = (hasGet ? $"System.Func<{typeSymbol}, {returnType}> get" : "") + (hasGet && hasSet ? ", " : "") + (hasSet ? $"System.Action<{typeSymbol}, {returnType}> set" : "");

        config.Documentation(doc => doc
                .Summary($"Configures the indexer for {typeSymbol.ToSeeCRef()} by specifying methods to call when the property is accessed.")
                .ParameterIf(hasGet, "get", "Function to call when the property is read.")
                .ParameterIf(hasSet, "set", "Function to call when the property is set.")
                .Returns("The configuration object."))
            .AddConfigMethod(context, "Indexer", [indexerParameters], builder => builder
                .AddIf(hasGet, () => $"target.{internalName}_get = get;")
                .AddIf(hasSet, () => $"target.{internalName}_set = set;")
            );
    }

    private void GenerateIndexerConfigExtensions(CodeBuilder codeBuilder, IPropertySymbol indexer)
    {
        var hasGet = indexer.GetMethod != null;
        var hasSet = indexer.SetMethod != null;

        var typeSymbol = indexer.Parameters[0].Type;

        codeBuilder
            .BR()
            .Documentation(doc => doc
                .Summary($"Specifies a dictionary to be use as a source of the indexer for {indexer.Parameters[0].Type.ToSeeCRef()}.")
                .Parameter("values", "Dictionary containing the values for the indexer.")
                .Returns("The updated configuration object."))
            .AddConfigExtension(context, indexer, [$"System.Collections.Generic.Dictionary<{typeSymbol}, {indexer.Type}> values"], builder => builder
                .AddIf(hasGet && hasSet, () => $"this.Indexer(get: ({typeSymbol} key) => values[key], set: ({typeSymbol} key, {indexer.Type} value) => values[key] = value);")
                .AddIf(hasGet && !hasSet, () => $"this.Indexer(get: ({typeSymbol} key) => values[key]);")
                .AddIf(!hasGet && hasSet, () => $"this.Indexer(set: ({typeSymbol} key, {indexer.Type} value) => values[key] = value);"));
    }
}
