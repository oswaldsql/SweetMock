namespace SweetMock.Builders.MemberBuilders;

using Generation;

/// <summary>
///     Represents a builder for indexers, implementing the ISymbolBuilder interface.
/// </summary>
internal partial class IndexBuilder(MockContext context)
{
    public static void Render(CodeBuilder classScope, MockContext context, IEnumerable<IPropertySymbol> symbols)
    {
        var builder = new IndexBuilder(context);
        builder.Build(classScope, symbols);
    }

    private void Build(CodeBuilder classScope, IEnumerable<IPropertySymbol> rawIndexers)
    {
        var indexers = rawIndexers.Select((indexer, i) => new IndexMedata(indexer, i + 1)).ToArray();

        if (indexers.Length == 0)
        {
            return;
        }

        this.CreateLogArgumentsRecord(classScope, indexers);
        this.BuildIndexes(classScope, indexers);
    }

    private void CreateLogArgumentsRecord(CodeBuilder classScope, IndexMedata[] indexers) =>
        classScope.Region("Indexers", builder =>
        {
            builder
                .Add("public record Indexer_Arguments(")
                .Indent(scope => scope
                    .Add("global::System.String? InstanceName,")
                    .Add("global::System.String MethodSignature,")
                    .Add($"{indexers.Select(t => t.Symbol).ToArray().GenerateKeyType()} key = null,")
                    .Add($"{indexers.Select(t => t.Symbol).ToArray().GenerateReturnType()} value = null")
                )
                .Add(") : ArgumentBase(_containerName, \"Indexer\", MethodSignature, InstanceName);")
                .BR();

            this.AddThrowConfiguration(builder, indexers);
        });

    private void BuildIndexes(CodeBuilder classScope, IEnumerable<IndexMedata> indexers)
    {
        foreach (var indexer in indexers)
        {
            classScope.Region($"Index : this[{indexer.KeyTypeString}]", builder =>
            {
                this.BuildIndex(builder, indexer);
                builder.AddToConfig(context, config =>
                {
                    this.AddGetSetConfiguration(config, indexer);
                    this.AddValuesConfiguration(config, indexer);
                });
            });
        }
    }

    private void BuildIndex(CodeBuilder classScope, IndexMedata indexer)
    {
        var signature = indexer.IsInInterface ?
            $"{indexer.ReturnTypeString} {indexer.ContainingSymbolString}.this[{indexer.KeyTypeString} {indexer.KeyName}]"
            : $"{indexer.AccessibilityString} override {indexer.ReturnTypeString} this[{indexer.KeyTypeString} {indexer.KeyName}]";

        classScope.Scope(signature, indexerScope => indexerScope
            .AddIf(indexer.HasGet, get => get
                .Scope("get", getScope => getScope
                    .Add($"this._log(new Indexer_Arguments(this._sweetMockInstanceName, \"get\", key : {indexer.KeyName}));")
                    .Add($"return this.{indexer.InternalName}_get({indexer.KeyName});")
                ))
            .AddIf(indexer.HasSet, set => set
                .Scope("set", setScope => setScope
                    .Add($"this._log(new Indexer_Arguments(this._sweetMockInstanceName, \"set\", key : {indexer.KeyName}, value : {indexer.TypeName}));")
                    .Add($"this.{indexer.InternalName}_set({indexer.KeyName}, value);")
                )))
            .BR()
            .AddIf(indexer.HasGet, () => $"private System.Func<{indexer.KeyTypeString}, {indexer.ReturnTypeString}> {indexer.InternalName}_get {{ get; set; }} = null!;")
            .AddIf(indexer.HasSet, () => $"private System.Action<{indexer.KeyTypeString}, {indexer.ReturnTypeString}> {indexer.InternalName}_set {{ get; set; }} = null!;")
            .BR();
    }

    private void AddGetSetConfiguration(CodeBuilder configScope, IndexMedata indexer)
    {
        var arguments = indexer switch
        {
            { IsGetSet: true } => $"System.Func<{indexer.KeyTypeString}, {indexer.ReturnTypeString}> get, System.Action<{indexer.KeyTypeString}, {indexer.ReturnTypeString}> set",
            { IsGetOnly: true } => $"System.Func<{indexer.KeyTypeString}, {indexer.ReturnTypeString}> get",
            { IsSetOnly: true } => $"System.Action<{indexer.KeyTypeString}, {indexer.ReturnTypeString}> set",
            _ => ""
        };

        configScope.Documentation(doc => doc
                .Summary($"Configures the indexer for {indexer.ToSeeCRef} by specifying methods to call when the property is accessed.")
                .ParameterIf(indexer.HasGet, "get", "Function to call when the property is read.")
                .ParameterIf(indexer.HasSet, "set", "Function to call when the property is set.")
                .Returns("The configuration object."))
            .AddConfigMethod(context, "Indexer", [arguments], builder => builder
                .AddIf(indexer.HasGet, () => $"target.{indexer.InternalName}_get = get;")
                .AddIf(indexer.HasSet, () => $"target.{indexer.InternalName}_set = set;")
            );
    }

    private void AddThrowConfiguration(CodeBuilder configScope, IndexMedata[] indexers) =>
        configScope.AddToConfig(context, builder => builder.Documentation(doc => doc
                .Summary("Configures all indexers to throw an exception when accessed.")
                .Parameter("throw", "Exception to throw when the indexer is accessed.")
                .Returns("The configuration object."))
            .AddConfigMethod(context, "Indexer", ["System.Exception throws"], codeBuilder =>
            {
                foreach (var indexer in indexers)
                {
                    codeBuilder
                        .AddIf(indexer.IsGetSet, () => $"this.Indexer(get : ({indexer.KeyTypeString} _) => throw throws, set : (_,_) => throw throws);")
                        .AddIf(indexer.IsGetOnly, () => $"this.Indexer(get : ({indexer.KeyTypeString} _) => throw throws);")
                        .AddIf(indexer.IsSetOnly, () => $"this.Indexer(set : ({indexer.KeyTypeString} _, {indexer.ReturnTypeString} _) => throw throws);");
                }
            }));

    private void AddValuesConfiguration(CodeBuilder configScope, IndexMedata indexer) =>
        configScope
            .BR()
            .Documentation(doc => doc
                .Summary($"Specifies a dictionary to be use as a source of the indexer for {indexer.ToSeeCRef}.")
                .Parameter("values", "Dictionary containing the values for the indexer.")
                .Returns("The updated configuration object."))
            .AddConfigMethod(context, "Indexer", [$"System.Collections.Generic.Dictionary<{indexer.KeyTypeString}, {indexer.ReturnTypeString}> values"], builder => builder
                .AddIf(indexer.IsGetSet, () => $"this.Indexer(get: ({indexer.KeyTypeString} key) => values[key], set: ({indexer.KeyTypeString} key, {indexer.ReturnTypeString} value) => values[key] = value);")
                .AddIf(indexer.IsGetOnly, () => $"this.Indexer(get: ({indexer.KeyTypeString} key) => values[key]);")
                .AddIf(indexer.IsSetOnly, () => $"this.Indexer(set: ({indexer.KeyTypeString} key, {indexer.ReturnTypeString} value) => values[key] = value);"));
}
