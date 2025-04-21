namespace SweetMock.Builders.MemberBuilders;

using System;
using System.Collections.Generic;
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

        using (classScope.Region($"#region Property : {name}"))
        {
            var index = 0;
            foreach (var symbol in symbols)
            {
                index++;
                BuildProperty(classScope, symbol, index);
            }
        }
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

        builder.AddLines($$"""

                      {{overwriteString.AccessibilityString}}{{overwriteString.OverrideString}}{{type}} {{overwriteString.ContainingSymbol}}{{propertyName}}
                      {
                      """).Indent();
        builder.Add(hasGet, () => $$"""
                                    get {
                                        {{LogBuilder.BuildLogSegment(symbol.GetMethod)}}
                                        return this._{{internalName}}_get();
                                    }
                                    """);
        builder.Add(hasSet, () => $$"""
                                    {{setType}} {
                                        {{LogBuilder.BuildLogSegment(symbol.SetMethod)}}
                                        this._{{internalName}}_set(value);
                                    }
                                    """);
        builder.Unindent().AddLines("}").AddLineBreak();

        builder.AddLines($$"""
                      private System.Func<{{type}}> _{{internalName}}_get { get; set; } = () => {{symbol.BuildNotMockedException()}}
                      private System.Action<{{type}}> _{{internalName}}_set { get; set; } = s => {{symbol.BuildNotMockedException()}}

                      """);

        using (builder.AddToConfig())
        {
            var p = (hasGet ? $"System.Func<{type}> get" : "") + (hasGet && hasSet ? ", ":"") + (hasSet ? $"System.Action<{type}> set" : "");

            builder.AddSummary($"Configures <see cref=\"{symbol.ToCRef()}\"/> by specifying methods to call when the property is accessed.");
            builder.AddParameter("get", "Function to call when the property is read.", hasGet);
            builder.AddParameter("set", "Function to call when the property is set.", hasGet);
            builder.AddReturns("The updated configuration object.");
            builder.AddLines($$"""public Config {{internalName}}({{p}}) {""").Indent();
            builder.Add(hasGet, () => $"target._{internalName}_get = get;");
            builder.Add(hasSet, () => $"target._{internalName}_set = set;");
            builder.AddLines("return this;");
            builder.Unindent().AddLines("}");
        }
    }

    public static void BuildConfigExtensions(CodeBuilder codeBuilder, MockDetails mock, IEnumerable<IPropertySymbol> properties)
    {
        foreach (var property in properties)
        {
            var hasGet = property.GetMethod != null;
            var hasSet = property.SetMethod != null;

            codeBuilder.AddSummary($"Specifies a value to used for mocking the property <see cref=\"{property.ToCRef()}\"/>.",
                "This method configures the mock to use the specified value when the property is accessed.");
            codeBuilder.AddParameter("returns", "The value to use for mocking the property.");
            codeBuilder.AddReturns("The updated configuration object.");
            codeBuilder.AddConfigExtension(mock, property, [$"{property.Type} returns"], builder =>
                {
                    builder.AddLines($"SweetMock.ValueBox<{property.Type}> {property.Name}_value = new (returns);");
                    builder.Add(hasGet && hasSet, () => $"this.{property.Name}(get : () => {property.Name}_value.Value, set : ({property.Type} value) => {property.Name}_value.Value = value);");
                    builder.Add(hasGet && !hasSet, () => $"this.{property.Name}(get : () => {property.Name}_value.Value);");
                    builder.Add(!hasGet && hasSet, () => $"this.{property.Name}(set : ({property.Type} value) => {property.Name}_value.Value = value);");
                }
            );
        }
    }
}
