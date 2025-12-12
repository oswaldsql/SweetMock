namespace SweetMock.Utils;

using Builders.MemberBuilders;
using Exceptions;

public static class Mappers
{
    internal static string ToString<T>(this IEnumerable<T>? values, Func<T, string> mapper, string separator = ", ") =>
        values == null ? "" : string.Join(separator, values.Select(mapper));

    internal static string ToCRef(this ISymbol symbol) =>
        symbol.ToDisplayString(Format.ToCRefFormat).Replace(".this[", ".Item[").Replace('<', '{').Replace('>', '}');

    public static string ToSeeCRef(this ISymbol symbol) =>
        $"""<see cref="global::{symbol.ToCRef()}">{symbol.ToDisplayString(Format.ToFullNameFormat).Replace("<", "&lt;").Replace(">", "&gt;")}</see>""";

    internal static string ToSeeCRef(this IEnumerable<MethodBuilder.MethodMetadata> methods) => string.Join(", ", methods.Select(t => t.ToSeeCRef));

    internal static string AccessibilityString(this ISymbol symbol) =>
        symbol.DeclaredAccessibility switch
        {
            Accessibility.Internal => "internal",
            Accessibility.NotApplicable => throw new UnsupportedAccessibilityException(symbol.DeclaredAccessibility),
            Accessibility.Private => "private",
            Accessibility.ProtectedAndInternal => "protected internal",
            Accessibility.Protected => "protected",
            Accessibility.ProtectedOrInternal => "protected",
            Accessibility.Public => "public",
            _ => throw new UnsupportedAccessibilityException(symbol.DeclaredAccessibility)
        };

    public static string AsNullable(this ISymbol symbol) =>
        $"{symbol}?";

    internal static string OutAsString(this IParameterSymbol parameterSymbol) =>
        parameterSymbol.RefKind switch
        {
            RefKind.Out => "out ",
            RefKind.Ref => "ref ",
            RefKind.In => "in ",
            RefKind.RefReadOnlyParameter => "ref readonly ",
            _ => ""
        };

    internal static string GetTypeGenerics(this INamedTypeSymbol type) =>
        type.IsGenericType
            ? "<" + string.Join(", ", type.TypeArguments.Select(t => t.ToDisplayString(Format.ToFullNameFormatWithGlobal))) + ">"
            : "";
}
