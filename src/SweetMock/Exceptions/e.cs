namespace SweetMock.Exceptions;

using System;
using Microsoft.CodeAnalysis;

internal class UnsupportedAccessibilityException(Accessibility accessibility) : Exception($"Unsupported accessibility type '{accessibility}'");

internal class RefPropertyNotSupportedException(IPropertySymbol propertySymbol, ITypeSymbol typeSymbol) : Exception($"Ref property not supported for '{propertySymbol.Name}' in '{typeSymbol.Name}'");

internal class RefReturnTypeNotSupportedException(IMethodSymbol methodSymbol, ITypeSymbol typeSymbol) : Exception($"Ref return type not supported for '{methodSymbol.Name}' in '{typeSymbol.Name}'");

internal class StaticAbstractMembersNotSupportedException(string name, ITypeSymbol typeSymbol) : Exception($"Static abstract members in interfaces or classes is not supported for '{name}' in '{typeSymbol.Name}'");

internal class CanNotMockASealedClassException(ITypeSymbol typeSymbol) : Exception($"Cannot mock the sealed class '{typeSymbol.Name}'");

