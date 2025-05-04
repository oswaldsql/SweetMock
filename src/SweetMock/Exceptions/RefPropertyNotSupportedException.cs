namespace SweetMock.Exceptions;

using System;
using Microsoft.CodeAnalysis;

internal class RefPropertyNotSupportedException(IPropertySymbol propertySymbol, ITypeSymbol typeSymbol) : Exception($"Ref property not supported for '{propertySymbol.Name}' in '{typeSymbol.Name}'");