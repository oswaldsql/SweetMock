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

    internal static string AccessibilityString(this ISymbol symbol) =>
        symbol.DeclaredAccessibility.AccessibilityString();

    internal static OverwriteString Overwrites(this ISymbol symbol)
    {
        if (symbol.ContainingType.TypeKind == TypeKind.Interface)
        {
            return new("global::" + symbol.ContainingSymbol + ".", "", "");
        }

        return new("", symbol.AccessibilityString() + " ", "override ");
    }

    public static string AsNullable(this ISymbol symbol) =>
        $"{symbol}?";

    public static string ToSeeCRef(this IEnumerable<ISymbol> symbols) => string.Join(", ", symbols.Distinct(SymbolEqualityComparer.Default).Select(t => t.ToSeeCRef()));

    public static string ToSeeCRef(this IEnumerable<MethodMetadata> methods) => methods.Select(t => t.Symbol).ToSeeCRef();

    private static string AccessibilityString(this Accessibility accessibility) =>
        accessibility switch
        {
            Accessibility.Internal => "internal",
            Accessibility.NotApplicable => throw new UnsupportedAccessibilityException(accessibility),
            Accessibility.Private => "private",
            Accessibility.ProtectedAndInternal => "protected internal",
            Accessibility.Protected => "protected",
            Accessibility.ProtectedOrInternal => "protected",
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

    internal static string GetTypeGenerics(this INamedTypeSymbol type) => type.IsGenericType ? "<" + string.Join(", ", type.TypeArguments.Select(t => t.ToDisplayString(Format.ToFullNameFormatWithGlobal))) + ">" : "";
}

public sealed class NamedSymbolEqualityComparer : IEqualityComparer<INamedTypeSymbol>
{
    private static readonly SymbolEqualityComparer SymbolEqualityComparer = SymbolEqualityComparer.Default;
    public static NamedSymbolEqualityComparer Default { get; } = new();

    public bool Equals(INamedTypeSymbol x, INamedTypeSymbol y) => SymbolEqualityComparer.Equals(x, y);

    public int GetHashCode(INamedTypeSymbol obj) => SymbolEqualityComparer.GetHashCode(obj);
}
