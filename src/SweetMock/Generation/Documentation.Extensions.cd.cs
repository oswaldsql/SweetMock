namespace SweetMock.Generation;

using Builders.MemberBuilders;
using Utils;

public static class Documentation_Extensions_cd
{
    internal static string ToCRef(this ISymbol symbol) =>
        symbol.ToDisplayString(Format.ToCRefFormat).Replace(".this[", ".Item[").Replace('<', '{').Replace('>', '}');

    public static string ToSeeCRef(this ISymbol symbol) =>
        $"""<see cref="global::{symbol.ToCRef()}">{symbol.ToDisplayString(Format.ToFullNameFormat).Replace("<", "&lt;").Replace(">", "&gt;")}</see>""";

    internal static string ToSeeCRef(this IEnumerable<MethodBuilder.MethodMetadata> methods) =>
        string.Join(", ", methods.Select(t => t.ToSeeCRef));

}
