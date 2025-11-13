namespace SweetMock.Utils;

using Exceptions;

public static class Mappers
{
    private static readonly SymbolDisplayFormat ToCRefFormat = new(
        SymbolDisplayGlobalNamespaceStyle.Omitted,
        SymbolDisplayTypeQualificationStyle.NameAndContainingTypesAndNamespaces,
        SymbolDisplayGenericsOptions.IncludeTypeParameters,
        SymbolDisplayMemberOptions.IncludeParameters | SymbolDisplayMemberOptions.IncludeContainingType,
        parameterOptions: SymbolDisplayParameterOptions.IncludeParamsRefOut | SymbolDisplayParameterOptions.IncludeType
    );

    private static readonly SymbolDisplayFormat ToFullNameFormat = new(
        SymbolDisplayGlobalNamespaceStyle.Omitted,
        SymbolDisplayTypeQualificationStyle.NameOnly,
        SymbolDisplayGenericsOptions.IncludeTypeParameters,
        SymbolDisplayMemberOptions.IncludeParameters | SymbolDisplayMemberOptions.IncludeContainingType,
        parameterOptions: SymbolDisplayParameterOptions.IncludeParamsRefOut | SymbolDisplayParameterOptions.IncludeType,
        miscellaneousOptions: SymbolDisplayMiscellaneousOptions.UseSpecialTypes
    );

    private static readonly SymbolDisplayFormat ToFullNameFormatWithGlobal = new(
        SymbolDisplayGlobalNamespaceStyle.Included,
        SymbolDisplayTypeQualificationStyle.NameAndContainingTypesAndNamespaces,
        SymbolDisplayGenericsOptions.IncludeTypeParameters,
        memberOptions: SymbolDisplayMemberOptions.IncludeParameters | SymbolDisplayMemberOptions.IncludeContainingType,
        parameterOptions: SymbolDisplayParameterOptions.IncludeParamsRefOut | SymbolDisplayParameterOptions.IncludeType
    );

    internal static string ToString<T>(this IEnumerable<T>? values, Func<T, string> mapper, string separator = ", ") =>
        values == null ? "" : string.Join(separator, values.Select(mapper));

    extension(ISymbol symbol)
    {
        internal string ToCRef() =>
            symbol.ToDisplayString(ToCRefFormat).Replace(".this[", ".Item[").Replace('<', '{').Replace('>', '}');

        public string ToSeeCRef() =>
            $"""<see cref="global::{symbol.ToCRef()}">{symbol.ToDisplayString(ToFullNameFormat).Replace("<", "&lt;").Replace(">", "&gt;")}</see>""";

        private string AccessibilityString() =>
            symbol.DeclaredAccessibility.AccessibilityString();

        internal OverwriteString Overwrites()
        {
            if (symbol.ContainingType.TypeKind == TypeKind.Interface)
            {
                return new("global::" + symbol.ContainingSymbol + ".", "", "");
            }

            return new("", symbol.AccessibilityString() + " ", "override ");
        }

        public string AsNullable() =>
            $"{symbol}?";
    }

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

    internal static ParameterStrings ParameterStrings(this IMethodSymbol method)
    {
        var parameters = method.Parameters.Select(t => new ParameterInfo(t.Type.ToString(), t.Name, t.OutAsString(), t.Name)).ToList();

        var methodParameters = parameters.ToString(p => $"{p.OutString}{p.Type} {p.Name}");

        if (method.IsGenericMethod)
        {
            parameters.AddRange(method.TypeArguments.Select(typeArgument => new ParameterInfo("System.Type", "typeOf_" + typeArgument.Name, "", "typeof(" + typeArgument.Name + ")")));
        }

        var nameList = parameters.ToString(p => $"{p.OutString}{p.Function}");

        return new(methodParameters, nameList);
    }

    internal static string GetTypeGenerics(this INamedTypeSymbol type) => type.IsGenericType ? "<" + string.Join(", ", type.TypeArguments.Select(t => t.ToDisplayString(ToFullNameFormatWithGlobal))) + ">" : "";
}

public sealed class NamedSymbolEqualityComparer : IEqualityComparer<INamedTypeSymbol>
{
    private static readonly SymbolEqualityComparer SymbolEqualityComparer = SymbolEqualityComparer.Default;
    public static NamedSymbolEqualityComparer Default { get; } = new();

    public bool Equals(INamedTypeSymbol x, INamedTypeSymbol y) => SymbolEqualityComparer.Equals(x, y);

    public int GetHashCode(INamedTypeSymbol obj) => SymbolEqualityComparer.GetHashCode(obj);
}
