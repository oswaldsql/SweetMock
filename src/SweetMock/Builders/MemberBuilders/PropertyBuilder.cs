namespace SweetMock.Builders.MemberBuilders;

using System;
using System.Collections.Generic;
using System.Diagnostics.SymbolStore;
using System.Linq;
using Microsoft.CodeAnalysis;
using Utils;

/// <summary>
///     Represents a builder for properties, implementing the ISymbolBuilder interface.
/// </summary>
internal static class PropertyBuilder
{
    public static void Build(CodeBuilder classScope, IEnumerable<IPropertySymbol> symbols)
    {
        var lookup = symbols.ToLookup(t => t.Name);
        foreach (var m in lookup)
        {
            BuildProperties(classScope, m.ToArray());
        }
    }

    /// <summary>
    ///     Builds properties and adds them to the code builder.
    /// </summary>
    /// <param name="classScope"></param>
    /// <param name="symbols">An array of property symbols representing the properties.</param>
    /// <returns>True if at least one property was built; otherwise, false.</returns>
    private static void BuildProperties(CodeBuilder classScope, IPropertySymbol[] symbols)
    {
        var name = symbols.First().Name;

        classScope.Region($"#region Property : {name}", builder =>
        {
            var index = 0;
            foreach (var symbol in symbols)
            {
                index++;
                BuildProperty(builder, symbol, index);
            }
        });
    }

    /// <summary>
    ///     Builds a property and adds it to the code builder.
    /// </summary>
    /// <param name="builder">The code builder to add the property to.</param>
    /// <param name="symbol">The property symbol representing the property.</param>
    /// <param name="index">The index of the property.</param>
    private static void BuildProperty(CodeBuilder builder, IPropertySymbol symbol, int index)
    {
        if(symbol.ReturnsByRef) throw new Exception("Property has returns byref");

        var propertyName = symbol.Name;
        var internalName = index == 1 ? propertyName : $"{propertyName}_{index}";
        var type = symbol.Type.ToString();
        var setType = symbol.SetMethod?.IsInitOnly == true ? "init" : "set";

        var overwriteString = symbol.Overwrites();

        var hasGet = symbol.GetMethod != null;
        var hasSet = symbol.SetMethod != null;

        builder.Scope($"{overwriteString.AccessibilityString}{overwriteString.OverrideString}{type} {overwriteString.ContainingSymbol}{propertyName}", b => b
                .Add(hasGet, get => get.Scope("get", getScope => getScope
                    .BuildLogSegment(symbol.GetMethod)
                    .Add($"return this._{internalName}_get();")))
                .Add(hasSet, set => set.Scope(setType, serScope => serScope
                    .BuildLogSegment(symbol.SetMethod)
                    .Add($"this._{internalName}_set(value);"))));

        builder
            .Add($"private System.Func<{type}> _{internalName}_get {{ get; set; }} = () => {symbol.BuildNotMockedException()}")
            .Add($"private System.Action<{type}> _{internalName}_set {{ get; set; }} = s => {symbol.BuildNotMockedException()}")
            .AddLineBreak();

        builder.AddToConfig(config =>
        {
            var p = (hasGet ? $"System.Func<{type}> get" : "") + (hasGet && hasSet ? ", " : "") + (hasSet ? $"System.Action<{type}> set" : "");

            config.Documentation(doc => doc
                .Summary($"Configures <see cref=\"{symbol.ToCRef()}\"/> by specifying methods to call when the property is accessed.")
                .Parameter("get", "Function to call when the property is read.", hasGet)
                .Parameter("set", "Function to call when the property is set.", hasGet)
                .Returns("The updated configuration object."));

            config
                .Add($$"""public Config {{internalName}}({{p}}) {""").Indent()
                .Add(hasGet, () => $"target._{internalName}_get = get;")
                .Add(hasSet, () => $"target._{internalName}_set = set;")
                .Add("return this;")
                .Unindent().Add("}");
        });
    }

    public static void BuildConfigExtensions(CodeBuilder codeBuilder, MockDetails mock, IEnumerable<IPropertySymbol> properties)
    {
        foreach (var property in properties)
        {
            var hasGet = property.GetMethod != null;
            var hasSet = property.SetMethod != null;

            codeBuilder.Documentation(doc => doc
                .Summary($"Specifies a value to used for mocking the property <see cref=\"{property.ToCRef()}\"/>.",
                    "This method configures the mock to use the specified value when the property is accessed.")
                .Parameter("value", "The value to use for the initial value of the property.")
                .Returns("The updated configuration object."));

            codeBuilder.AddConfigExtension(mock, property, [$"{property.Type} value"], builder =>
                {
                    builder.Add($"SweetMock.ValueBox<{property.Type}> {property.Name}_value = new (value);");
                    builder.Add(hasGet && hasSet, () => $"this.{property.Name}(get : () => {property.Name}_value.Value, set : ({property.Type} value) => {property.Name}_value.Value = value);");
                    builder.Add(hasGet && !hasSet, () => $"this.{property.Name}(get : () => {property.Name}_value.Value);");
                    builder.Add(!hasGet && hasSet, () => $"this.{property.Name}(set : ({property.Type} value) => {property.Name}_value.Value = value);");
                }
            );
        }
    }
}
