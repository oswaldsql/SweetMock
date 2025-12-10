namespace SweetMock.Builders.MemberBuilders;

using Utils;

internal static class MethodBuilderHelpers
{
    internal static ILookup<ISymbol?, MethodMetadata> GroupByReturnType(this IEnumerable<MethodMetadata> infos)
        => infos.ToLookup(t => t.ReturnType, SymbolEqualityComparer.Default);

    private static bool IsMethodTypeParameter(this ITypeSymbol type) =>
        type is ITypeParameterSymbol { TypeParameterKind: TypeParameterKind.Method, DeclaringMethod: not null };

    internal static bool IsReturnTypeDerivedFromGeneric(this IMethodSymbol symbol)
    {
        var returnType = symbol.ReturnType as INamedTypeSymbol;
        return returnType?.TypeArguments.Any(IsMethodTypeParameter) == true;
    }

    internal static IEnumerable<IParameterSymbol> OutParameters(this IMethodSymbol symbol) =>
        symbol.Parameters.Where(t => t.RefKind == RefKind.Out);

    internal static bool HasOutParameters(this IMethodSymbol symbol) =>
        symbol.Parameters.Any(t => t.RefKind == RefKind.Out);

    internal static IEnumerable<ParameterInfo> GetParameterInfos(this IMethodSymbol symbol)
    {
        if (!symbol.IsGenericMethod)
        {
            foreach (var t in symbol.Parameters)
            {
                yield return new(t.Type.ToString(), t.Name, t.OutAsString(), t.Name);
            }

            yield break;
        }

        foreach (var parameter in symbol.Parameters)
        {
            if (parameter.Type is { TypeKind: TypeKind.TypeParameter, ContainingSymbol: IMethodSymbol })
            {
                yield return new("System.Object", parameter.Name, parameter.OutAsString(), parameter.Name);
            }
            else if (((INamedTypeSymbol)parameter.Type).IsGenericType)
            {
                yield return new("System.Object", parameter.Name, parameter.OutAsString(), parameter.Name);
            }
            else
            {
                yield return new(parameter.Type.ToString(), parameter.Name, parameter.OutAsString(), parameter.Name);
            }
        }

        foreach (var typeArgument in symbol.TypeArguments)
        {
            yield return new("System.Type", "typeOf_" + typeArgument.Name, "", "typeof(" + typeArgument.Name + ")");
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
