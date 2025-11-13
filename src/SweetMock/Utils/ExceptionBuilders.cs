namespace SweetMock.Utils;

public static class ExceptionBuilders
{
    extension(ISymbol symbol)
    {
        internal string BuildNotMockedException()
            => $"throw new SweetMock.NotExplicitlyMockedException(\"{symbol.Name}\", \"{symbol.ContainingType.Name}\");";
    }

    extension(IPropertySymbol symbol)
    {
        public string BuildNotMockedExceptionForIndexer()
            => $"throw new SweetMock.NotExplicitlyMockedException(\"{symbol.Name}\", \"{symbol.ContainingType.Name}\");";
    }
}
