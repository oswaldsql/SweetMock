#nullable enable
namespace SweetMock;

using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;

/// <summary>
/// Factory for creating mock objects.
/// </summary>
[System.CodeDom.Compiler.GeneratedCode("SweetMock","{{SweetMockVersion}}")]
internal static partial class Mock
{
}

[System.CodeDom.Compiler.GeneratedCode("SweetMock","{{SweetMockVersion}}")]
internal static partial class Fixture{}

public class ValueBox<T>
{
    public ValueBox(T value) =>
        this.Value = value;
    public T Value { get; set; }
}

public class MockOptions
{
    public MockOptions(CallLog? logger = null) =>
        this.Logger = logger;

    public static MockOptions Default => new();

    public CallLog? Logger { get; init; } = null;
}
