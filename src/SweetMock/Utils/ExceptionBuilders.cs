namespace SweetMock.Utils;

public static class ExceptionBuilders
{
    internal static string BuildNotMockedException(this IPropertySymbol symbol)
        => $"throw new System.InvalidOperationException(\"The property '{symbol.Name}' in '{symbol.ContainingType.Name}' is not explicitly mocked.\") {{Source = \"{symbol}\"}};";

    public static string BuildNotMockedExceptionForIndexer(this IPropertySymbol symbol) =>
        $"throw new System.InvalidOperationException(\"The indexer '{symbol.Name}' in '{symbol.ContainingType.Name}' is not explicitly mocked.\") {{Source = \"{symbol}\"}};";

    internal static string BuildNotMockedException(this IMethodSymbol symbol)
        => $"throw new System.InvalidOperationException(\"The method '{symbol.Name}' in '{symbol.ContainingType.Name}' is not explicitly mocked.\") {{Source = \"{symbol}\"}};";
}