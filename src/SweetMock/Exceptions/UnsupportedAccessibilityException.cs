namespace SweetMock.Exceptions;

using System;
using Microsoft.CodeAnalysis;

internal class UnsupportedAccessibilityException(Accessibility accessibility) : Exception($"Unsupported accessibility type '{accessibility}'");