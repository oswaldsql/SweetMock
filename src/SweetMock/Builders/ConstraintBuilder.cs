namespace SweetMock.Builders;

public static class ConstraintBuilder
{
    public static string ToConstraints(this INamedTypeSymbol symbol) =>
        string.Join(" ", symbol.TypeArguments.OfType<ITypeParameterSymbol>().Select(ToConstraintString));

    /// <summary>
    ///     Converts a type parameter symbol to a constraint string.
    /// </summary>
    /// <param name="symbol">The type parameter symbol to convert.</param>
    /// <returns>A string representing the constraints for the type parameter.</returns>
    private static string ToConstraintString(ITypeParameterSymbol symbol)
    {
        var results = string.Join(", ", ToConstraintElements(symbol));

        return results.Length == 0 ? "" : " where " + symbol.Name + " : " + results;
    }

    private static IEnumerable<string> ToConstraintElements(ITypeParameterSymbol symbol)
    {
        foreach (var ct in symbol.ConstraintTypes.Select(t => t.ToDisplayString()))
        {
            yield return ct;
        }

        if (symbol.HasUnmanagedTypeConstraint)
        {
            yield return "unmanaged";
        }
        else
        {
            if (symbol.HasConstructorConstraint)
            {
                yield return "new()";
            }

            if (symbol.HasValueTypeConstraint)
            {
                yield return "struct";
            }
        }

        if (symbol.HasReferenceTypeConstraint)
        {
            yield return "class";
        }

        if (symbol.HasNotNullConstraint)
        {
            yield return "notnull";
        }
    }
}
