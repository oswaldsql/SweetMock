namespace SweetMock.Utils;

/// <summary>
/// Provides a collection of preconfigured <see cref="SymbolDisplayFormat"/> instances
/// used for converting symbols into various string representations.
/// </summary>
public static class Format
{
    /// <summary>
    /// A format that generates strings suitable for XML cref documentation tags,
    /// which reference code elements like methods, types, or members.
    /// <para>Example: <c>MyNamespace.MyClass.MyMethod(System.Int32 param)</c></para>
    /// </summary>
    internal static readonly SymbolDisplayFormat ToCRefFormat = new(
        SymbolDisplayGlobalNamespaceStyle.Omitted,
        SymbolDisplayTypeQualificationStyle.NameAndContainingTypesAndNamespaces,
        SymbolDisplayGenericsOptions.IncludeTypeParameters,
        SymbolDisplayMemberOptions.IncludeParameters | SymbolDisplayMemberOptions.IncludeContainingType,
        parameterOptions: SymbolDisplayParameterOptions.IncludeParamsRefOut | SymbolDisplayParameterOptions.IncludeType
    );

    /// <summary>
    /// A format generating a simple representation of fully qualified names
    /// for types and members, including type parameters.
    /// <para>Example: <c>MyMethod(param1, param2)</c></para>
    /// </summary>
    internal static readonly SymbolDisplayFormat ToFullNameFormat = new(
        SymbolDisplayGlobalNamespaceStyle.Omitted,
        SymbolDisplayTypeQualificationStyle.NameOnly,
        SymbolDisplayGenericsOptions.IncludeTypeParameters,
        SymbolDisplayMemberOptions.IncludeParameters | SymbolDisplayMemberOptions.IncludeContainingType,
        parameterOptions: SymbolDisplayParameterOptions.IncludeParamsRefOut | SymbolDisplayParameterOptions.IncludeType,
        miscellaneousOptions: SymbolDisplayMiscellaneousOptions.UseSpecialTypes
    );

    /// <summary>
    /// A format for producing fully qualified names that include the global namespace.
    /// <para>Example: <c>global::MyNamespace.MyClass.MyMethod</c></para>
    /// </summary>
    internal static readonly SymbolDisplayFormat ToFullNameFormatWithGlobal = new(
        SymbolDisplayGlobalNamespaceStyle.Included,
        SymbolDisplayTypeQualificationStyle.NameAndContainingTypesAndNamespaces,
        SymbolDisplayGenericsOptions.IncludeTypeParameters,
        SymbolDisplayMemberOptions.IncludeParameters | SymbolDisplayMemberOptions.IncludeContainingType,
        parameterOptions: SymbolDisplayParameterOptions.IncludeParamsRefOut | SymbolDisplayParameterOptions.IncludeType,
        miscellaneousOptions: SymbolDisplayMiscellaneousOptions.IncludeNullableReferenceTypeModifier
    );

    internal static readonly SymbolDisplayFormat ToFullNameFormatWithGlobalWithoutNull = new(
        SymbolDisplayGlobalNamespaceStyle.Included,
        SymbolDisplayTypeQualificationStyle.NameAndContainingTypesAndNamespaces,
        SymbolDisplayGenericsOptions.IncludeTypeParameters,
        SymbolDisplayMemberOptions.IncludeParameters | SymbolDisplayMemberOptions.IncludeContainingType,
        parameterOptions: SymbolDisplayParameterOptions.IncludeParamsRefOut | SymbolDisplayParameterOptions.IncludeType
    );

    /// <summary>
    /// A format that includes the type name and generic type arguments but excludes namespaces.
    /// <para>Example: <c>MyClass&lt;T&gt;</c></para>
    /// </summary>
    internal static readonly SymbolDisplayFormat NameAndGenerics = new(
        typeQualificationStyle: SymbolDisplayTypeQualificationStyle.NameAndContainingTypes,
        genericsOptions: SymbolDisplayGenericsOptions.IncludeTypeParameters
    );

    /// <summary>
    /// An extended format including fully qualified names, generic type constraints,
    /// variances, and method parameters with their names and types.
    /// <para>Example: <c>global::MyNamespace.MyClass&lt;T&gt;(int param1, string param2)</c></para>
    /// </summary>
    internal static readonly SymbolDisplayFormat ExtendedTypeFormat = new(
        SymbolDisplayGlobalNamespaceStyle.Included,
        SymbolDisplayTypeQualificationStyle.NameAndContainingTypesAndNamespaces,
        SymbolDisplayGenericsOptions.IncludeTypeConstraints | SymbolDisplayGenericsOptions.IncludeTypeParameters | SymbolDisplayGenericsOptions.IncludeVariance,
        SymbolDisplayMemberOptions.IncludeContainingType | SymbolDisplayMemberOptions.IncludeParameters,
        parameterOptions: SymbolDisplayParameterOptions.IncludeType | SymbolDisplayParameterOptions.IncludeName,
        miscellaneousOptions: SymbolDisplayMiscellaneousOptions.EscapeKeywordIdentifiers
    );

    /// <summary>
    /// A format that only includes the signature of members such as methods,
    /// including their names, generic type arguments, and parameters.
    /// <para>Example: <c>MyMethod(int param1, string param2)</c></para>
    /// </summary>
    internal static readonly SymbolDisplayFormat SignatureOnlyFormat = new(
        SymbolDisplayGlobalNamespaceStyle.Omitted,
        SymbolDisplayTypeQualificationStyle.NameAndContainingTypesAndNamespaces,
        SymbolDisplayGenericsOptions.IncludeTypeParameters,
        SymbolDisplayMemberOptions.IncludeParameters,
        parameterOptions: SymbolDisplayParameterOptions.IncludeType | SymbolDisplayParameterOptions.IncludeParamsRefOut,
        miscellaneousOptions: SymbolDisplayMiscellaneousOptions.None
    );

    /// <summary>
    /// A format that produces fully qualified names with the global namespace,
    /// used for referencing members with explicit namespace inclusion.
    /// <para>Example: <c>global::MyNamespace.MyClass.MyMethod(int param)</c></para>
    /// </summary>
    internal static readonly SymbolDisplayFormat ToFullNameFormat2 = new(
        SymbolDisplayGlobalNamespaceStyle.Included,
        SymbolDisplayTypeQualificationStyle.NameAndContainingTypesAndNamespaces,
        SymbolDisplayGenericsOptions.IncludeTypeParameters,
        SymbolDisplayMemberOptions.IncludeParameters | SymbolDisplayMemberOptions.IncludeContainingType,
        parameterOptions: SymbolDisplayParameterOptions.IncludeParamsRefOut | SymbolDisplayParameterOptions.IncludeType
    );

    /// <summary>
    /// A format similar to <see cref="ToFullNameFormat2"/>, but excludes generic type arguments.
    /// <para>Example: <c>global::MyNamespace.MyClass.MyMethod</c></para>
    /// </summary>
    internal static readonly SymbolDisplayFormat ToFullNameFormatWithoutGeneric = new(
        SymbolDisplayGlobalNamespaceStyle.Included,
        SymbolDisplayTypeQualificationStyle.NameAndContainingTypesAndNamespaces,
        SymbolDisplayGenericsOptions.None,
        SymbolDisplayMemberOptions.IncludeParameters | SymbolDisplayMemberOptions.IncludeContainingType,
        parameterOptions: SymbolDisplayParameterOptions.IncludeParamsRefOut | SymbolDisplayParameterOptions.IncludeType
    );

    /// <summary>
    /// A default and unconfigured format with no specific display settings applied.
    /// <para>Example: (Empty string due to the lack of configuration)</para>
    /// </summary>
    internal static readonly SymbolDisplayFormat Format2 = new();
}
