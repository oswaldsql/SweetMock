namespace SweetMock.Exceptions;

internal class RefReturnTypeNotSupportedException(IMethodSymbol methodSymbol, ITypeSymbol typeSymbol) : SweetMockException($"Ref return type not supported for '{methodSymbol.Name}' in '{typeSymbol.Name}'");
