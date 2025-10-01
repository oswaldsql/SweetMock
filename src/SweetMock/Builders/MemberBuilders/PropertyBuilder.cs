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

    public void Build(CodeBuilder classScope, IEnumerable<IPropertySymbol> symbols)
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
            var index = 0;
            foreach (var symbol in symbols)
            {
                index++;
                this.BuildProperty(builder, symbol, index);
            }
        });
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
                    .BuildLogSegment(context, symbol.GetMethod)
                    .Scope($"if (this._{internalName}_get is null)", ifScope =>
                    {
                        ifScope.Add($"throw new SweetMock.NotExplicitlyMockedException(\"{symbol.Name}\", _sweetMockInstanceName);");
                    })
                    .Add($"return this._{internalName}_get();")))
            .AddIf(hasSet, set => set
                .Scope(setType, setScope => setScope
                    .BuildLogSegment(context, symbol.SetMethod)
                    .Scope($"if (this._{internalName}_set is null)", ifScope =>
                    {
                        ifScope.Add($"throw new SweetMock.NotExplicitlyMockedException(\"{symbol.Name}\", _sweetMockInstanceName);");
                    })
                    .Add($"this._{internalName}_set(value);"))));

        builder
            .Add($"private System.Func<{type}>? _{internalName}_get {{ get; set; }} = null;")
            .Add($"private System.Action<{type}>? _{internalName}_set {{ get; set; }} = null;")
            .AddLineBreak();

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

        codeBuilder.AddConfigExtension(context, property, [$"{property.Type} value"], builder =>
            {
                builder.Add($"SweetMock.ValueBox<{property.Type}> {property.Name}_value = new (value);");
                builder.AddIf(hasGet && hasSet, () => $"this.{property.Name}(get : () => {property.Name}_value.Value, set : ({property.Type} value) => {property.Name}_value.Value = value);");
                builder.AddIf(hasGet && !hasSet, () => $"this.{property.Name}(get : () => {property.Name}_value.Value);");
                builder.AddIf(!hasGet && hasSet, () => $"this.{property.Name}(set : ({property.Type} value) => {property.Name}_value.Value = value);");
            }
        );
    }
}
