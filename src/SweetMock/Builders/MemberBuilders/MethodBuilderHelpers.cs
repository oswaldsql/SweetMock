namespace SweetMock.Builders.MemberBuilders;

using Utils;

internal static class MethodBuilderHelpers
{
    public static readonly SymbolDisplayFormat CustomSymbolDisplayFormat = new(
        globalNamespaceStyle: SymbolDisplayGlobalNamespaceStyle.Included,
        typeQualificationStyle: SymbolDisplayTypeQualificationStyle.NameAndContainingTypesAndNamespaces,
        genericsOptions: SymbolDisplayGenericsOptions.IncludeTypeConstraints | SymbolDisplayGenericsOptions.IncludeTypeParameters | SymbolDisplayGenericsOptions.IncludeVariance,
        memberOptions: SymbolDisplayMemberOptions.IncludeContainingType | SymbolDisplayMemberOptions.IncludeParameters,
        parameterOptions: SymbolDisplayParameterOptions.IncludeType |SymbolDisplayParameterOptions.IncludeName,
        miscellaneousOptions: SymbolDisplayMiscellaneousOptions.EscapeKeywordIdentifiers
    );

    public static readonly SymbolDisplayFormat SignatureOnlyFormat = new(
        globalNamespaceStyle: SymbolDisplayGlobalNamespaceStyle.Omitted,
        typeQualificationStyle: SymbolDisplayTypeQualificationStyle.NameAndContainingTypesAndNamespaces,
        genericsOptions: SymbolDisplayGenericsOptions.IncludeTypeParameters,
        memberOptions: SymbolDisplayMemberOptions.IncludeParameters, // Exclude method name, containing type, etc.
        parameterOptions: SymbolDisplayParameterOptions.IncludeType | SymbolDisplayParameterOptions.IncludeParamsRefOut,
        miscellaneousOptions: SymbolDisplayMiscellaneousOptions.None // No nullability or other modifiers
    );

    extension(ITypeSymbol type)
    {
        private bool IsMethodTypeParameter() =>
            type is ITypeParameterSymbol { TypeParameterKind: TypeParameterKind.Method, DeclaringMethod: not null };
    }

    extension(IEnumerable<IMethodSymbol> candidate)
    {
        internal bool IsReturnTypeDerivedFromGeneric()
        {
            var typeSymbol = candidate.First().ReturnType as INamedTypeSymbol;
            var nop = typeSymbol?.TypeArguments.Any(IsMethodTypeParameter);
            return nop == true;
        }
    }

    extension(IMethodSymbol symbol)
    {
        internal MethodBuilder.DelegateInfo GetDelegateInfo(int methodCount)
        {
            var delegateName = methodCount == 1 ? $"DelegateFor_{symbol.Name}" : $"DelegateFor_{symbol.Name}_{methodCount}";
            var delegateType = symbol is { IsGenericMethod: true, ReturnsVoid: false } ? "object" : symbol.ReturnType.ToString();

            var parameterList = GetParameterInfos(symbol).ToString(p => $"{p.OutString}{p.Type} {p.Name}");

            return new(delegateName, delegateType, parameterList);
        }

        internal IEnumerable<ParameterInfo> GetParameterInfos()
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

        internal MethodBuilder.MethodInfo MethodMetadata()
        {
            var methodName = symbol.Name;
            var methodReturnType = symbol.ReturnType.ToString();
            var returnString = symbol.ReturnsVoid ? "" : "return ";

            return new(methodName, methodReturnType, returnString);
        }

        internal string GenericString()
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
}
