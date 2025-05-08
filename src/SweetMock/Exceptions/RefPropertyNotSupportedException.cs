namespace SweetMock.Exceptions;

internal class RefPropertyNotSupportedException(IPropertySymbol propertySymbol, ITypeSymbol typeSymbol) : SweetMockException($"Ref property not supported for '{propertySymbol.Name}' in '{typeSymbol.Name}'");
