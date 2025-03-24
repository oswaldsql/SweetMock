namespace SweetMock.Builders.MemberBuilders;

using System;
using System.Linq;
using Microsoft.CodeAnalysis;
using Utils;

/// <summary>
///     Represents a builder for properties, implementing the ISymbolBuilder interface.
/// </summary>
internal class PropertyBuilder : IBaseClassBuilder, ILoggingExtensionBuilder
{
    public bool TryBuildBase(MockDetails details, CodeBuilder result, ISymbol[] symbols)
    {
        var handled = IsHandled(symbols);
        if (handled.HasValue)
        {
            return handled.Value;
        }
        
        var propertySymbols = symbols.OfType<IPropertySymbol>().Where(t => !t.IsIndexer).ToArray();
        return BuildProperties(result, propertySymbols, details);
    }

    public bool? IsHandled(ISymbol[] symbols)
    {
        var first = symbols.First();

        if (first is IMethodSymbol { MethodKind: MethodKind.PropertyGet or MethodKind.PropertySet }) return true;

        if (first is not IPropertySymbol symbol || symbol.IsIndexer) return false;

        if (!(first.IsAbstract || first.IsVirtual)) return true;

        if (symbol.ReturnsByRef || symbol.ReturnsByRefReadonly) return false;

        if (symbols.OfType<IPropertySymbol>().All(t => t.IsIndexer)) return false;

        return null;
    }
    
    public bool TryBuildLoggingExtension(MockDetails details, CodeBuilder result, ISymbol[] symbols)
    {
        var handled = IsHandled(symbols);
        if (handled.HasValue)
        {
            return handled.Value;
        }

        return false;
    }
    
    /// <summary>
    ///     Builds helper methods for the property.
    /// </summary>
    /// <param name="symbol">The property symbol representing the property.</param>
    /// <param name="type">The type of the property.</param>
    /// <param name="internalName">The internal name of the property.</param>
    /// <param name="propertyName">The name of the property.</param>
    /// <returns>A collection of helper methods for the property.</returns>
//    private static IEnumerable<ConfigExtension> BuildHelpers(IPropertySymbol symbol, string type, string internalName, string propertyName)
//    {
//        ConfigExtension cx(string signature, string code, string documentation, [CallerLineNumber] int ln = 0) => new(signature, code + "// line : " + ln, documentation,  symbol.ToString());
//
//        var hasGet = symbol.GetMethod != null;
//        var hasSet = symbol.SetMethod != null;
//
//        var isNullable = symbol.NullableAnnotation == NullableAnnotation.Annotated;
//        if (isNullable)
//        {
//            yield return cx(
//                $"{type.Replace("?", "")} value"
//                , $"""
//                   target._{internalName} = value;
//                   target._{internalName}_get = () => target._{internalName};
//                   target._{internalName}_set = s => target._{internalName} = s;
//                   """,
//                $"Sets an initial value for {propertyName}.");
//        }
//        else
//        {
//            yield return cx(
//                $"{type.Replace("?", "")} value"
//                , $"""
//                   target._{internalName} = value;
//                   target._{internalName}_get = () => target._{internalName} ?? {symbol.BuildNotMockedException()};
//                   target._{internalName}_set = s => target._{internalName} = s;
//                   """,
//                $"Sets an initial value for {propertyName}.");
//        }
//
//        switch (hasSet, hasGet)
//        {
//            case (true, true):
//                yield return cx(
//                    $"System.Func<{type}> get, System.Action<{type}> set",
//                    $"""
//                     target._{internalName}_get = get;
//                     target._{internalName}_set = set;
//                     """,
//                    $"Specifies a getter and setter method to call when the property {propertyName} is called.");
//                break;
//            case (false, true):
//                yield return cx(
//                    $"System.Func<{type}> get",
//                    $"target._{internalName}_get = get;",
//                    $"Specifies a getter method to call when the property {propertyName} is called.");
//                break;
//            case (true, false):
//                yield return cx(
//                    $"System.Action<{type}> set",
//                    $"target._{internalName}_set = set;",
//                    $"Specifies a setter method to call when the property {propertyName} is set.");
//                break;
//        }
//    }


    /// <summary>
    ///     Builds properties and adds them to the code builder.
    /// </summary>
    /// <param name="builder">The code builder to add the properties to.</param>
    /// <param name="symbols">An array of property symbols representing the properties.</param>
    /// <param name="details"></param>
    /// <returns>True if at least one property was built; otherwise, false.</returns>
    private static bool BuildProperties(CodeBuilder builder, IPropertySymbol[] symbols, MockDetails details)
    {
        var name = symbols.First().Name;

        builder.Add($"#region Property : {name}").Indent();

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
                BuildProperty(builder, symbol, index, details);
            }

        builder.Unindent().Add("#endregion");

        return index > 0;
    }

    /// <summary>
    ///     Builds a property and adds it to the code builder.
    /// </summary>
    /// <param name="builder">The code builder to add the property to.</param>
    /// <param name="symbol">The property symbol representing the property.</param>
    /// <param name="index">The index of the property.</param>
    /// <param name="details">Details of the mock created.</param>
    private static void BuildProperty(CodeBuilder builder, IPropertySymbol symbol, int index, MockDetails details)
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
                      private {{type.TrimEnd('?')}}? _{{internalName}};
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