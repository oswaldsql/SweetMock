namespace SweetMock.Utils;

using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;

public static class Mappers
{
    internal static string ToString<T>(this IEnumerable<T>? values, Func<T, string> mapper, string separator = ", ") =>
        values == null ? "" : string.Join(separator, values.Select(mapper));

    private static readonly SymbolDisplayFormat Format = new(
        SymbolDisplayGlobalNamespaceStyle.Omitted,
        SymbolDisplayTypeQualificationStyle.NameAndContainingTypesAndNamespaces,
        SymbolDisplayGenericsOptions.IncludeTypeParameters,
        memberOptions: SymbolDisplayMemberOptions.IncludeParameters | SymbolDisplayMemberOptions.IncludeContainingType,
        parameterOptions: SymbolDisplayParameterOptions.IncludeParamsRefOut | SymbolDisplayParameterOptions.IncludeType
    );

    public static string ToCRef(this ISymbol symbol)
    {
        var prefix = symbol.Prefix();

        return $"{symbol.ToDisplayString(Format)}".Replace(".this[",".Item[").Replace('<', '{').Replace('>', '}');
    }

    private static string Prefix(this ISymbol symbol) =>
        symbol switch
        {
            INamespaceSymbol => "N:",
            ITypeSymbol => "T:",
            IFieldSymbol => "F:",
            IPropertySymbol => "P:",
            IMethodSymbol => "M:",
            IEventSymbol => "E:",
            _ => ""
        };

    internal static string AccessibilityString(this ISymbol method) =>
        method.DeclaredAccessibility.AccessibilityString();

    private static string AccessibilityString(this Accessibility accessibility) =>
        accessibility switch
        {
            Accessibility.Internal => "internal",
            Accessibility.NotApplicable => throw new UnsupportedAccessibilityException(accessibility),
            Accessibility.Private => "private",
            Accessibility.ProtectedAndInternal => "protected internal",
            Accessibility.Protected => "protected",
            Accessibility.ProtectedOrInternal => "protected internal",
            Accessibility.Public => "public",
            _ => throw new UnsupportedAccessibilityException(accessibility)
        };

    internal static string OutAsString(this IParameterSymbol parameterSymbol) =>
        parameterSymbol.RefKind switch
        {
            RefKind.Out => "out ",
            RefKind.Ref => "ref ",
            RefKind.In => "in ",
            RefKind.RefReadOnlyParameter => "ref readonly ",
            _ => ""
        };

    internal static ParameterStrings ParameterStrings(this IMethodSymbol method)
    {
        var parameters = method.Parameters.Select(t => new ParameterInfo(t.Type.ToString(), t.Name, t.OutAsString(), t.Name)).ToList();

        var methodParameters = parameters.ToString(p => $"{p.OutString}{p.Type} {p.Name}");

        if (method.IsGenericMethod)
        {
            parameters.AddRange(method.TypeArguments.Select(typeArgument => new ParameterInfo("System.Type", "typeOf_" + typeArgument.Name, "", "typeof(" + typeArgument.Name + ")")));
        }

        var parameterList = parameters.ToString(p => $"{p.OutString}{p.Type} {p.Name}");
        var typeList = parameters.ToString(p => $"{p.Type}");
        var nameList = parameters.ToString(p => $"{p.OutString}{p.Function}");

        return new ParameterStrings (methodParameters, parameterList, typeList, nameList);
    }

    internal static OverwriteString Overwrites(this ISymbol symbol)
    {
        if (symbol.ContainingType.TypeKind == TypeKind.Interface)
        {
            return new(symbol.ContainingSymbol + ".", "", "");
        }

        return new("", symbol.AccessibilityString() + " ", "override ");
    }
}
