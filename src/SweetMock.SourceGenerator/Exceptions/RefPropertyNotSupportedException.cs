namespace SweetMock.Exceptions;

using Builders.MemberBuilders;

internal class RefPropertyNotSupportedException(PropertyBuilder.PropertyMetadata metadata) : SweetMockException($"Ref property not supported for '{metadata.Name}' in '{metadata.Symbol.ContainingSymbol.Name}'");
