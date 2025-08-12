namespace SweetMock.Utils;

public static class ExceptionBuilders
{
    internal static string BuildNotMockedException(this ISymbol symbol)
        => $"throw new SweetMock.NotExplicitlyMockedException(\"{symbol.Name}\", \"{symbol.ContainingType.Name}\");";

    public static string BuildNotMockedExceptionForIndexer(this IPropertySymbol symbol)
        => $"throw new SweetMock.NotExplicitlyMockedException(\"{symbol.Name}\", \"{symbol.ContainingType.Name}\");";
}
