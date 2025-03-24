namespace SweetMock.Builders;

using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;

public static class ConstraintBuilder
{
    /// <summary>
    ///     Converts an array of type arguments to a constraints string.
    /// </summary>
    /// <param name="typeArguments">The type arguments to convert.</param>
    /// <returns>A string representing the constraints.</returns>
    public static string ToConstraints(this ImmutableArray<ITypeSymbol> typeArguments) => 
        string.Join(" ", typeArguments.OfType<ITypeParameterSymbol>().Select(ToConstraintString));

    /// <summary>
    ///     Converts a type parameter symbol to a constraint string.
    /// </summary>
    /// <param name="symbol">The type parameter symbol to convert.</param>
    /// <returns>A string representing the constraints for the type parameter.</returns>
    private static string ToConstraintString(ITypeParameterSymbol symbol)
    {
        var results = string.Join(", ", symbol.ToConstraintElements());

        return results.Length == 0 ? "" : " where " + symbol.Name + " : " + results;
    }

    private static IEnumerable<string> ToConstraintElements(this ITypeParameterSymbol symbol)
    {
        foreach (var ct in symbol.ConstraintTypes.Select(t => t.ToString()))
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
