namespace SweetMock.Builders;

using Microsoft.CodeAnalysis;
using Utils;

/// <summary>
///     Interface for building code based on a grouping of symbols.
/// </summary>
internal interface IBaseClassBuilder
{
    /// <summary>
    ///     Attempts to build a symbol using the provided CodeBuilder and a grouping of symbols.
    /// </summary>
    /// <param name="details">Details on the mock to build.</param>
    /// <param name="result">The CodeBuilder instance to append the result to.</param>
    /// <param name="symbols">A grouping of symbols to be used in the building process.</param>
    /// <returns>True if the symbol was successfully built; otherwise, false.</returns>
    public bool TryBuildBase(MockDetails details, CodeBuilder result, ISymbol[] symbols);
}
