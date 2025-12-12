namespace SweetMock.Utils;

using Exceptions;

public static class Mappers
{
    internal static string Combine<T>(this IEnumerable<T>? values, Func<T, string> mapper, string separator = ", ") =>
        values == null ? "" : string.Join(separator, values.Select(mapper));


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

    public static string AsNullable(this ITypeSymbol symbol) =>
        $"{symbol.ToDisplayString(Format.ToFullNameFormatWithGlobal)}" + (symbol.NullableAnnotation == NullableAnnotation.Annotated ? "" : "?");

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
