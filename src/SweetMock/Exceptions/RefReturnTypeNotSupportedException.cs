namespace SweetMock.Exceptions;

using Builders.MemberBuilders;

internal class RefReturnTypeNotSupportedException(MethodBuilder.MethodMetadata metadata) :
    SweetMockException($"Ref return type not supported for '{metadata.Name}' in '{metadata.Mock.Name}'");
