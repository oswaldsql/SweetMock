namespace SweetMock.Builders.MemberBuilders;

using Utils;

internal static class MethodBuilderHelpers
{
    private static bool IsMethodTypeParameter(this ITypeSymbol type) =>
        type is ITypeParameterSymbol tp &&
        tp.TypeParameterKind == TypeParameterKind.Method &&
        tp.DeclaringMethod is not null;

    internal static bool IsReturnTypeDerivedFromGeneric(this IEnumerable<IMethodSymbol> candidate)
    {
        var typeSymbol = candidate.First().ReturnType as INamedTypeSymbol;
        var nop = typeSymbol?.TypeArguments.Any(IsMethodTypeParameter);
        return nop == true;
    }

    internal static MethodBuilder.DelegateInfo GetDelegateInfo(this IMethodSymbol symbol, int methodCount)
    {
        var delegateName = methodCount == 1 ? $"DelegateFor_{symbol.Name}" : $"DelegateFor_{symbol.Name}_{methodCount}";
        var delegateType = symbol is { IsGenericMethod: true, ReturnsVoid: false } ? "object" : symbol.ReturnType.ToString();

        var parameterList = GetParameterInfos(symbol).ToString(p => $"{p.OutString}{p.Type} {p.Name}");

        return new(delegateName, delegateType, parameterList);
    }

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

    internal static MethodBuilder.MethodInfo MethodMetadata(this IMethodSymbol method)
    {
        var methodName = method.Name;
        var methodReturnType = method.ReturnType.ToString();
        var returnString = method.ReturnsVoid ? "" : "return ";

        return new(methodName, methodReturnType, returnString);
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
