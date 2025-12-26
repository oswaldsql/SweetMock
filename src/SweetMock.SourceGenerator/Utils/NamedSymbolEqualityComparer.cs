namespace SweetMock.Utils;

internal sealed class NamedSymbolEqualityComparer : IEqualityComparer<INamedTypeSymbol?>
{
    private static readonly SymbolEqualityComparer SymbolEqualityComparer = SymbolEqualityComparer.Default;
    public static NamedSymbolEqualityComparer Default { get; } = new();

    public bool Equals(INamedTypeSymbol? x, INamedTypeSymbol? y) => SymbolEqualityComparer.Equals(x, y);

    public int GetHashCode(INamedTypeSymbol? obj) => SymbolEqualityComparer.GetHashCode(obj);
}

internal sealed class TypeSymbolEqualityComparer : IEqualityComparer<ITypeSymbol?>
{
    private static readonly SymbolEqualityComparer SymbolEqualityComparer = SymbolEqualityComparer.Default;
    public static TypeSymbolEqualityComparer Default { get; } = new();

    public bool Equals(ITypeSymbol? x, ITypeSymbol? y) => SymbolEqualityComparer.Equals(x, y);

    public int GetHashCode(ITypeSymbol? obj) => SymbolEqualityComparer.GetHashCode(obj);
}
