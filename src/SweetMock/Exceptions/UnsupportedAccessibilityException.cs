namespace SweetMock.Utils;

using System;
using Microsoft.CodeAnalysis;

internal class UnsupportedAccessibilityException(Accessibility accessibility) : Exception($"Unsupported accessibility type '{accessibility}'")
{
    public Accessibility Accessibility => accessibility;
}

internal class InvalidTargetTypeException(TypeKind targetTypeKind) : Exception
{
    private readonly TypeKind targetTypeKind = targetTypeKind;
}

internal class InvalidAccessibilityException(Accessibility targetDeclaredAccessibility) : Exception
{
    private readonly Accessibility targetDeclaredAccessibility = targetDeclaredAccessibility;
}
