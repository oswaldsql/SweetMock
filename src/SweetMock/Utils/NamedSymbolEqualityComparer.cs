namespace SweetMock.Utils;

public sealed class NamedSymbolEqualityComparer : IEqualityComparer<INamedTypeSymbol?>
{
    private static readonly SymbolEqualityComparer SymbolEqualityComparer = SymbolEqualityComparer.Default;
    public static NamedSymbolEqualityComparer Default { get; } = new();

    public bool Equals(INamedTypeSymbol? x, INamedTypeSymbol? y) => SymbolEqualityComparer.Equals(x, y);

    public int GetHashCode(INamedTypeSymbol? obj) => SymbolEqualityComparer.GetHashCode(obj);
}
