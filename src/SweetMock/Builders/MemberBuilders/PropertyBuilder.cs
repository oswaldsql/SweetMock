namespace SweetMock.Builders.MemberBuilders;

using Exceptions;
using Generation;

/// <summary>
///     Represents a builder for properties, implementing the ISymbolBuilder interface.
/// </summary>
internal partial class PropertyBuilder(MockContext context)
{
    public static void Render(CodeBuilder classScope, MockContext context, IEnumerable<IPropertySymbol> symbols)
    {
        var builder = new PropertyBuilder(context);
        builder.Build(classScope,symbols);
    }

    private void Build(CodeBuilder classScope, IEnumerable<IPropertySymbol> symbols)
    {
        var lookup = symbols.ToLookup(t => t.Name);
        foreach (var m in lookup)
        {
            this.BuildProperties(classScope, m);
        }
    }

    /// <summary>
    ///     Builds properties and adds them to the code builder.
    /// </summary>
    /// <param name="classScope"></param>
    /// <param name="symbols">An array of property symbols representing the properties.</param>
    /// <returns>True if at least one property was built; otherwise, false.</returns>
    private void BuildProperties(CodeBuilder classScope, IGrouping<string, IPropertySymbol> symbols) =>
        classScope.Region($"#region Property : {symbols.Key}", builder =>
        {
            CreateLogArgumentsRecord(classScope, symbols.ToArray(), symbols.Key);

            foreach (var symbol in symbols)
            {
                if (symbol.ReturnsByRef)
                {
                    throw new RefPropertyNotSupportedException(symbol, symbol.ContainingType);
                }

                var p = new PropertyMedata(symbol);

                this.BuildProperty(builder, p);
                this.BuildConfigExtensions(builder, p);
            }
        });

    private static void CreateLogArgumentsRecord(CodeBuilder classScope, IPropertySymbol[] symbols, string name) =>
        classScope
            .Add($"public record {name}_Arguments(")
            .Indent(scope => scope
                .Add("global::System.String? InstanceName,")
                .Add("global::System.String MethodSignature,")
                .Add($"{symbols.GenerateReturnType()} value = null")
            )
            .Add($") : ArgumentBase(_containerName, \"{name}\", MethodSignature, InstanceName);")
            .BR();

    /// <summary>
    ///     Builds a property and adds it to the code builder.
    /// </summary>
    /// <param name="builder">The code builder to add the property to.</param>
    /// <param name="symbol">The property symbol representing the property.</param>
    private void BuildProperty(CodeBuilder builder, PropertyMedata p)
    {
        var setType = p.IsInitOnly ? "init" : "set";

        var signature = p.Symbol.ContainingType.TypeKind == TypeKind.Interface ?
            $"{p.TypeString} {p.ContainingSymbolString}.{p.Name}"
            : $"{p.AccessibilityString} override {p.TypeString} {p.Name}";

        builder
            .Scope(signature, propertyScope => propertyScope
                .AddIf(p.HasGet, get => get
                    .Scope("get", getScope => getScope
                        .Add($"this._log(new {p.Name}_Arguments(this._sweetMockInstanceName, \"get\"));")
                        .Add($"return this._{p.Name}_get();")))
                .AddIf(p.HasSet, set => set
                    .Scope(setType, setScope => setScope
                        .Add($"this._log(new {p.Name}_Arguments(this._sweetMockInstanceName, \"set\", value : value));")
                        .Add($"this._{p.Name}_set(value);"))))
            .BR()
            .AddIf(p.HasGet, () => $"private System.Func<{p.TypeString}> _{p.Name}_get {{ get; set; }} = null!;")
            .AddIf(p.HasSet, () => $"private System.Action<{p.TypeString}> _{p.Name}_set {{ get; set; }} = null!;")
            .BR();
    }

    private void BuildConfigExtensions(CodeBuilder builder, PropertyMedata property) =>
        builder.AddToConfig(context, config =>
        {
            this.AddGetSetConfiguration(config, property);
            this.AddThrowConfiguration(config, property);
            this.AddValueConfiguration(config, property);
        });

    private void AddGetSetConfiguration(CodeBuilder config, PropertyMedata p)
    {
        var arguments = p switch
        {
            { IsGetSet: true } => $"global::System.Func<{p.TypeString}> get, global::System.Action<{p.TypeString}> set",
            { IsGetOnly: true } => $"global::System.Func<{p.TypeString}> get",
            { IsSetOnly: true } => $"global::System.Action<{p.TypeString}> set",
            _ => ""
        };

        config.Documentation(doc => doc
                .Summary($"Configures {p.ToSeeCRef} by specifying methods to call when the property is accessed.")
                .ParameterIf(p.HasGet, "get", "Function to call when the property is read.")
                .ParameterIf(p.HasSet, "set", "Function to call when the property is set.")
                .Returns("The updated configuration object."))
            .AddConfigMethod(context, p.Name, [arguments], codeBuilder => codeBuilder
                .AddIf(p.HasGet, () => $"target._{p.Name}_get = get;")
                .AddIf(p.HasSet, () => $"target._{p.Name}_set = set;")
            );
    }

    private void AddThrowConfiguration(CodeBuilder config, PropertyMedata p) => config
        .Documentation(doc => doc
            .Summary("Configures the mock to throw the specified exception when the property is accessed.", $"Configures {p.ToSeeCRef}")
            .Parameter("throws", "The exception to be thrown when the property is accessed.")
            .Returns("The updated configuration object."))
        .AddConfigMethod(context, p.Name, ["System.Exception throws"], codeBuilder => codeBuilder
            .AddIf(p.IsGetSet, () => $"this.{p.Name}(get : () => throw throws, set : _ => throw throws);")
            .AddIf(p.IsGetOnly, () => $"this.{p.Name}(get : () => throw throws);")
            .AddIf(p.IsSetOnly, () => $"this.{p.Name}(set : _ => throw throws);")
        );

    private void AddValueConfiguration(CodeBuilder config, PropertyMedata p) => config
        .Documentation(doc => doc
            .Summary($"Specifies a value to use for mocking the property {p.ToSeeCRef}.")
            .Parameter("value", "The value to use for the initial value of the property.")
            .Returns("The updated configuration object."))
        .AddConfigMethod(context, p.Name, [$"{p.TypeString} value"], builder => builder
            .Add($"global::SweetMock.ValueBox<{p.TypeString}> {p.Name}_value = new (value);")
            .AddIf(p.IsGetSet, () => $"this.{p.Name}(get : () => {p.Name}_value.Value, set : ({p.TypeString} value) => {p.Name}_value.Value = value);")
            .AddIf(p.IsGetOnly, () => $"this.{p.Name}(get : () => {p.Name}_value.Value);")
            .AddIf(p.IsSetOnly, () => $"this.{p.Name}(set : ({p.TypeString} value) => {p.Name}_value.Value = value);"));
}
