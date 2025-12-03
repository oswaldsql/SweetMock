namespace SweetMock.Builders.MemberBuilders;

using Exceptions;
using Generation;
using Utils;

/// <summary>
///     Represents a builder for properties, implementing the ISymbolBuilder interface.
/// </summary>
internal class PropertyBuilder(MockContext context)
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
            this.BuildProperties(classScope, m.ToArray());
        }
    }

    /// <summary>
    ///     Builds properties and adds them to the code builder.
    /// </summary>
    /// <param name="classScope"></param>
    /// <param name="symbols">An array of property symbols representing the properties.</param>
    /// <returns>True if at least one property was built; otherwise, false.</returns>
    private void BuildProperties(CodeBuilder classScope, IPropertySymbol[] symbols)
    {
        var name = symbols.First().Name;

        classScope.Region($"#region Property : {name}", builder =>
        {
            CreateLogArgumentsRecord(classScope, symbols, name);

            foreach (var symbol in symbols)
            {
                this.BuildProperty(builder, symbol);
                this.BuildConfigExtensions(builder, symbol);
            }
        });
    }

    private static void CreateLogArgumentsRecord(CodeBuilder classScope, IPropertySymbol[] symbols, string name) =>
        classScope
            .Add($"public record {name}_Arguments(")
            .Indent(scope => scope
                .Add("global::System.String? InstanceName,")
                .Add("global::System.String MethodSignature,")
                .Add($"{GenerateArgumentType(symbols)} value = null")
            )
            .Add($") : ArgumentBase(_containerName, \"{name}\", MethodSignature, InstanceName);")
            .BR();

    private static string GenerateArgumentType(IPropertySymbol[] symbols)
    {
        if (symbols.Length > 1)
        {
            return "global::System.Object?";
        }

        var type = symbols.First().Type;

        if (type is ITypeParameterSymbol)
        {
            return "global::System.Object?";
        }
        if (type is INamedTypeSymbol namedType && namedType.IsGenericType)
        {
            return "global::System.Object?";
        }
        if (type.NullableAnnotation != NullableAnnotation.Annotated)
        {
            return type.ToDisplayString(MethodBuilderHelpers.SignatureOnlyFormat) + "?";
        }
        return type.ToDisplayString(MethodBuilderHelpers.SignatureOnlyFormat);
    }

    /// <summary>
    ///     Builds a property and adds it to the code builder.
    /// </summary>
    /// <param name="builder">The code builder to add the property to.</param>
    /// <param name="symbol">The property symbol representing the property.</param>
    private void BuildProperty(CodeBuilder builder, IPropertySymbol symbol)
    {
        if (symbol.ReturnsByRef)
        {
            throw new RefPropertyNotSupportedException(symbol, symbol.ContainingType);
        }

        var propertyName = symbol.Name;
        var type = symbol.Type.ToString();
        var setType = symbol.SetMethod?.IsInitOnly == true ? "init" : "set";

        var overwriteString = symbol.Overwrites();

        var hasGet = symbol.GetMethod != null;
        var hasSet = symbol.SetMethod != null;

        var signature = $"{overwriteString.AccessibilityString}{overwriteString.OverrideString}{type} {overwriteString.ContainingSymbol}{propertyName}";
        builder
            .Scope(signature, propertyScope => propertyScope
                .AddIf(hasGet, get => get
                    .Scope("get", getScope => getScope
                        .Add($"this._log(new {propertyName}_Arguments(this._sweetMockInstanceName, \"get\"));")
                        .Scope($"if (this._{propertyName}_get is null)", ifScope => ifScope
                            .Add($"throw new global::SweetMock.NotExplicitlyMockedException(\"{propertyName}\", this._sweetMockInstanceName);"))
                        .Add($"return this._{propertyName}_get();")))
                .AddIf(hasSet, set => set
                    .Scope(setType, setScope => setScope
                        .Add($"this._log(new {propertyName}_Arguments(this._sweetMockInstanceName, \"set\", value : value));")
                        .Scope($"if (this._{propertyName}_set is null)", ifScope => ifScope
                            .Add($"throw new global::SweetMock.NotExplicitlyMockedException(\"{propertyName}\", this._sweetMockInstanceName);"))
                        .Add($"this._{propertyName}_set(value);"))))
            .BR()
            .AddIf(hasGet, () => $"private System.Func<{type}>? _{propertyName}_get {{ get; set; }} = null;")
            .AddIf(hasSet, () => $"private System.Action<{type}>? _{propertyName}_set {{ get; set; }} = null;")
            .BR();
    }

    private void BuildConfigExtensions(CodeBuilder builder, IPropertySymbol symbol) =>
        builder.AddToConfig(context, config =>
        {
            this.AddGetSetConfiguration(config, symbol);

            this.AddValueConfiguration(config, symbol);
        });

    private void AddGetSetConfiguration(CodeBuilder config, IPropertySymbol symbol)
    {
        var hasGet = symbol.GetMethod != null;
        var hasSet = symbol.SetMethod != null;
        var propertyType = symbol.Type.ToString();
        var name = symbol.Name;

        var p = (hasGet ? $"System.Func<{propertyType}> get" : "") + (hasGet && hasSet ? ", " : "") + (hasSet ? $"System.Action<{propertyType}> set" : "");

        config.Documentation(doc => doc
                .Summary($"Configures {symbol.ToSeeCRef()} by specifying methods to call when the property is accessed.")
                .ParameterIf(hasGet, "get", "Function to call when the property is read.")
                .ParameterIf(hasSet, "set", "Function to call when the property is set.")
                .Returns("The updated configuration object."))
            .AddConfigMethod(context, name, [p], codeBuilder => codeBuilder
                .AddIf(hasGet, () => $"target._{name}_get = get;")
                .AddIf(hasSet, () => $"target._{name}_set = set;")
            );
    }

    private void AddValueConfiguration(CodeBuilder codeBuilder, IPropertySymbol property)
    {
        var hasGet = property.GetMethod != null;
        var hasSet = property.SetMethod != null;

        var propertyType = property.Type;

        codeBuilder.Documentation(doc => doc
                .Summary($"Specifies a value to use for mocking the property {property.ToSeeCRef()}.")
                .Parameter("value", "The value to use for the initial value of the property.")
                .Returns("The updated configuration object."))
            .AddConfigExtension(context, property, [$"{propertyType} value"], builder => builder
                .Add($"global::SweetMock.ValueBox<{propertyType}> {property.Name}_value = new (value);")
                .AddIf(hasGet && hasSet, () => $"this.{property.Name}(get : () => {property.Name}_value.Value, set : ({propertyType} value) => {property.Name}_value.Value = value);")
                .AddIf(hasGet && !hasSet, () => $"this.{property.Name}(get : () => {property.Name}_value.Value);")
                .AddIf(!hasGet && hasSet, () => $"this.{property.Name}(set : ({propertyType} value) => {property.Name}_value.Value = value);"));
    }
}
