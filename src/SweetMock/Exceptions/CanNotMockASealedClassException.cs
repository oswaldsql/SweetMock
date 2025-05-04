namespace SweetMock.Exceptions;

using System;
using Microsoft.CodeAnalysis;

internal class CanNotMockASealedClassException(ITypeSymbol typeSymbol) : Exception($"Cannot mock the sealed class '{typeSymbol.Name}'");