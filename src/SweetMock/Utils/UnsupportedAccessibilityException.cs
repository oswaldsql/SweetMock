namespace SweetMock.Utils;

using System;
using Microsoft.CodeAnalysis;

internal class UnsupportedAccessibilityException(Accessibility accessibility) : Exception($"Unsupported accessibility type '{accessibility}'")
{
    public Accessibility Accessibility => accessibility;
}