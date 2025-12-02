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
            classScope
                .Add($"public record {name}_Arguments(")
                .Indent(scope => scope
                    .Add("global::System.String? InstanceName,")
                    .Add("global::System.String MethodSignature,")
                    .Add($"{GenerateArgumentType(symbols)} value = null")
                )
                .Add($") : ArgumentBase(_containerName, \"{name}\", MethodSignature, InstanceName);")
                .BR();

            var index = 0;
            foreach (var symbol in symbols)
            {
                index++;
                this.BuildProperty(builder, symbol, index);
            }
        });
    }

    private static string GenerateArgumentType(IPropertySymbol[] symbols)
    {
        var symbol = symbols.First();
        var type = symbol.Type;

        if (type is ITypeParameterSymbol)
        {
            return "object?";
        }
        if (type is INamedTypeSymbol namedType && namedType.IsGenericType)
        {
            return "object?";
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
    /// <param name="index">The index of the property.</param>
    private void BuildProperty(CodeBuilder builder, IPropertySymbol symbol, int index)
    {
        if (symbol.ReturnsByRef)
        {
            throw new RefPropertyNotSupportedException(symbol, symbol.ContainingType);
        }

        var propertyName = symbol.Name;
        var internalName = index == 1 ? propertyName : $"{propertyName}_{index}";
        var type = symbol.Type.ToString();
        var setType = symbol.SetMethod?.IsInitOnly == true ? "init" : "set";

        var overwriteString = symbol.Overwrites();

        var hasGet = symbol.GetMethod != null;
        var hasSet = symbol.SetMethod != null;

        var signature = $"{overwriteString.AccessibilityString}{overwriteString.OverrideString}{type} {overwriteString.ContainingSymbol}{propertyName}";
        builder.Scope(signature, propertyScope => propertyScope
            .AddIf(hasGet, get => get
                .Scope("get", getScope => getScope
                    .Add($"this._log(new {symbol.Name}_Arguments(this._sweetMockInstanceName, \"get\"));")
                    .Scope($"if (this._{internalName}_get is null)", ifScope => ifScope
                        .Add($"throw new global::SweetMock.NotExplicitlyMockedException(\"{symbol.Name}\", this._sweetMockInstanceName);"))
                    .Add($"return this._{internalName}_get();")))
            .AddIf(hasSet, set => set
                .Scope(setType, setScope => setScope
                    .Add($"this._log(new {symbol.Name}_Arguments(this._sweetMockInstanceName, \"set\", value : value));")
                    .Scope($"if (this._{internalName}_set is null)", ifScope => ifScope
                        .Add($"throw new global::SweetMock.NotExplicitlyMockedException(\"{symbol.Name}\", this._sweetMockInstanceName);"))
                    .Add($"this._{internalName}_set(value);"))));

        builder
            .Add($"private System.Func<{type}>? _{internalName}_get {{ get; set; }} = null;")
            .Add($"private System.Action<{type}>? _{internalName}_set {{ get; set; }} = null;")
            .BR();

        builder.AddToConfig(context, config =>
        {
            var p = (hasGet ? $"System.Func<{type}> get" : "") + (hasGet && hasSet ? ", " : "") + (hasSet ? $"System.Action<{type}> set" : "");

            config.Documentation(doc => doc
                .Summary($"Configures {symbol.ToSeeCRef()} by specifying methods to call when the property is accessed.")
                .Parameter("get", "Function to call when the property is read.", hasGet)
                .Parameter("set", "Function to call when the property is set.", hasGet)
                .Returns("The updated configuration object."));

            config.AddConfigMethod(context, internalName, [p], codeBuilder => codeBuilder
                .AddIf(hasGet, () => $"target._{internalName}_get = get;")
                .AddIf(hasSet, () => $"target._{internalName}_set = set;")
            );

            this.GeneratePropertyMockConfiguration(config, symbol);
        });
    }

    private void GeneratePropertyMockConfiguration(CodeBuilder codeBuilder, IPropertySymbol property)
    {
        var hasGet = property.GetMethod != null;
        var hasSet = property.SetMethod != null;

        codeBuilder.Documentation(doc => doc
            .Summary($"Specifies a value to use for mocking the property {property.ToSeeCRef()}.")
            .Parameter("value", "The value to use for the initial value of the property.")
            .Returns("The updated configuration object."));

        codeBuilder.AddConfigExtension(context, property, [$"{property.Type} value"], builder => builder
            .Add($"global::SweetMock.ValueBox<{property.Type}> {property.Name}_value = new (value);")
            .AddIf(hasGet && hasSet, () => $"this.{property.Name}(get : () => {property.Name}_value.Value, set : ({property.Type} value) => {property.Name}_value.Value = value);")
            .AddIf(hasGet && !hasSet, () => $"this.{property.Name}(get : () => {property.Name}_value.Value);")
            .AddIf(!hasGet && hasSet, () => $"this.{property.Name}(set : ({property.Type} value) => {property.Name}_value.Value = value);"));
    }
}
