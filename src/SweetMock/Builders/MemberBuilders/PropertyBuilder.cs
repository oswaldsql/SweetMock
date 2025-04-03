namespace SweetMock.Builders.MemberBuilders;

using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;
using Utils;

/// <summary>
///     Represents a builder for properties, implementing the ISymbolBuilder interface.
/// </summary>
internal class PropertyBuilder
{
    public static CodeBuilder Build(IEnumerable<IPropertySymbol> symbols)
    {
        CodeBuilder result = new();

        var lookup = symbols.ToLookup(t => t.Name);
        foreach (var m in lookup)
        {
            result.Add(BuildProperties(m.ToArray()));
        }

        return result;
    }

    /// <summary>
    ///     Builds properties and adds them to the code builder.
    /// </summary>
    /// <param name="symbols">An array of property symbols representing the properties.</param>
    /// <returns>True if at least one property was built; otherwise, false.</returns>
    private static CodeBuilder BuildProperties(IPropertySymbol[] symbols)
    {
        CodeBuilder builder = new();
        var name = symbols.First().Name;

        using (builder.Region($"#region Property : {name}"))
        {
            var index = 0;
            foreach (var symbol in symbols)
            {
                index++;
                BuildProperty(builder, symbol, index);
            }

            return builder;
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

        builder.Add($$"""

                      {{overwriteString.accessibilityString}}{{overwriteString.overrideString}}{{type}} {{overwriteString.containingSymbol}}{{propertyName}}
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
        builder.Unindent().Add("}").Add();

        builder.Add($$"""
                      //private {{type.TrimEnd('?')}}? _{{internalName}};
                      private System.Func<{{type}}> _{{internalName}}_get { get; set; } = () => {{symbol.BuildNotMockedException()}}
                      private System.Action<{{type}}> _{{internalName}}_set { get; set; } = s => {{symbol.BuildNotMockedException()}}

                      """);

        using (builder.AddToConfig())
        {
            var p = (hasGet ? $"System.Func<{type}> get" : "") + (hasGet && hasSet ? ", ":"") + (hasSet ? $"System.Action<{type}> set" : "");

            builder.AddSummary($"Configures {internalName} by specifying methods to call when the property is accessed.");
            builder.AddParameter("get", "Function to call when the property is read.", hasGet);
            builder.AddParameter("set", "Function to call when the property is set.", hasGet);
            builder.AddReturns("The updated configuration object.");
            builder.Add($$"""public Config {{internalName}}({{p}}) {""").Indent();
            builder.Add(hasGet, () => $"target._{internalName}_get = get;");
            builder.Add(hasSet, () => $"target._{internalName}_set = set;");
            builder.Add("return this;");
            builder.Unindent().Add($$"""}""");
        }
    }

    public static CodeBuilder BuildConfigExtensions(MockDetails mock, IEnumerable<IPropertySymbol> properties)
    {
        var result = new CodeBuilder();

        foreach (var property in properties)
        {
            var hasGet = property.GetMethod != null;
            var hasSet = property.SetMethod != null;

            result.AddSummary($"Specifies a value to used for mocking the property <see cref=\"{property.ToCRef()}\"/>.",
                "This method configures the mock to use the specified value when the property is accessed.");
            result.AddParameter("config", "The configuration object used to set up the mock.");
            result.AddParameter("returns", "The value to use for mocking the property.");
            result.AddReturns("The updated configuration object.");
            result.Add($"public static {mock.MockType}.Config {property.Name}(this {mock.MockType}.Config config, {property.Type} returns)");
            result.Add("{").Indent();
            result.Add($"SweetMock.ValueBox<{property.Type}> {property.Name}_value = new (returns);");
            result.Add(hasGet && hasSet, () => $"config.{property.Name}(get : () => {property.Name}_value.Value, set : ({property.Type} value) => {property.Name}_value.Value = value);");
            result.Add(hasGet && !hasSet, () =>$"config.{property.Name}(get : () => {property.Name}_value.Value);");
            result.Add(!hasGet && hasSet, () =>$"config.{property.Name}(set : ({property.Type} value) => {property.Name}_value.Value = value);");
            result.Add("return config;");
            result.Unindent().Add("}");
        }

        return result;
    }
}
