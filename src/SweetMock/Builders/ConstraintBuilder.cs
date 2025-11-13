namespace SweetMock.Builders;

public static class ConstraintBuilder
{
    extension(INamedTypeSymbol symbol)
    {
        public string ToConstraints() =>
            string.Join(" ", symbol.TypeArguments.OfType<ITypeParameterSymbol>().Select(ToConstraintString));
    }

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

    extension(ITypeParameterSymbol symbol)
    {
        private IEnumerable<string> ToConstraintElements()
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
}
