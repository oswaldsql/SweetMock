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
                if (symbol.IsStatic)
                {
                    builder.Add($"// Ignoring Static property {symbol}.");
                }
                else if (symbol is { IsAbstract: false, IsVirtual: false })
                {
                    builder.Add($"// Ignoring property {symbol}.");
                }
                else
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

        builder.Add("internal partial class Config {");

        switch (hasSet, hasGet)
        {
            case (true, true):
                builder.Add($$"""
                                public Config {{internalName}}(System.Func<{{type}}> get, System.Action<{{type}}> set) {
                                    target._{{internalName}}_get = get;
                                    target._{{internalName}}_set = set;
                                    return this;
                                }
                              """);
                break;
            case (false, true):
                builder.Add($$"""
                                public Config {{internalName}}(System.Func<{{type}}> get) {
                                    target._{{internalName}}_get = get;
                                    return this;
                                }
                              """);
                break;
            case (true, false):
                builder.Add($$"""
                                public Config {{internalName}}(System.Action<{{type}}> set) {
                                    target._{{internalName}}_set = set;
                                    return this;
                                }
                              """);
                break;
        }

        builder.Add("}");
    }
}
