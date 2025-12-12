namespace SweetMock.Exceptions;

using Builders.MemberBuilders;

internal class RefPropertyNotSupportedException(PropertyBuilder.PropertyMetadata medata) : SweetMockException($"Ref property not supported for '{medata.Name}' in '{medata.Symbol.ContainingSymbol.Name}'");
