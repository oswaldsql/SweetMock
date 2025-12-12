namespace SweetMock.Builders.MemberBuilders;

using Utils;

internal static class MethodBuilderHelpers
{
    internal static ILookup<ISymbol?, MethodBuilder.MethodMetadata> GroupByReturnType(this IEnumerable<MethodBuilder.MethodMetadata> infos)
        => infos.ToLookup(t => t.ReturnType, SymbolEqualityComparer.Default);

    private static bool IsMethodTypeParameter(this ITypeSymbol type) =>
        type is ITypeParameterSymbol { TypeParameterKind: TypeParameterKind.Method, DeclaringMethod: not null };

    internal static bool IsReturnTypeDerivedFromGeneric(this IMethodSymbol symbol) =>
        (symbol.ReturnType as INamedTypeSymbol)?.TypeArguments.Any(IsMethodTypeParameter) == true;

    internal static IEnumerable<ParameterInfo> GetParameterInfos(this IMethodSymbol symbol)
    {
        foreach (var parameter in symbol.Parameters)
        {
            yield return parameter.Type switch
            {
                { TypeKind: TypeKind.TypeParameter, ContainingSymbol: IMethodSymbol } => new("global::System.Object?", parameter.Name, parameter.OutAsString(), parameter.Name),
                INamedTypeSymbol typeSymbol when typeSymbol.IsGenericType && typeSymbol.Name != "Nullable" => new("global::System.Object?", parameter.Name, parameter.OutAsString(), parameter.Name),
                _ => new(parameter.Type.ToDisplayString(Format.ToFullNameFormatWithGlobal), parameter.Name, parameter.OutAsString(), parameter.Name)
            };
        }

        foreach (var typeArgument in symbol.TypeArguments)
        {
            yield return new("global::System.Type", "typeOf_" + typeArgument.Name, "", "typeof(" + typeArgument.Name + ")");
        }
    }

    internal static string GenericString(this IMethodSymbol symbol)
    {
        if (!symbol.IsGenericMethod)
        {
            return "";
        }

        var typeArguments = symbol.TypeArguments;
        var types = string.Join(", ", typeArguments.Select(t => t.Name));
        return $"<{types}>";
    }
}
