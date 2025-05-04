namespace SweetMock.Exceptions;

using System;
using Microsoft.CodeAnalysis;

internal class RefReturnTypeNotSupportedException(IMethodSymbol methodSymbol, ITypeSymbol typeSymbol) : Exception($"Ref return type not supported for '{methodSymbol.Name}' in '{typeSymbol.Name}'");