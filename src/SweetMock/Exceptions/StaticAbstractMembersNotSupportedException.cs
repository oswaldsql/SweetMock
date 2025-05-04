namespace SweetMock.Exceptions;

using System;
using Microsoft.CodeAnalysis;

internal class StaticAbstractMembersNotSupportedException(string name, ITypeSymbol typeSymbol) : Exception($"Static abstract members in interfaces or classes is not supported for '{name}' in '{typeSymbol.Name}'");