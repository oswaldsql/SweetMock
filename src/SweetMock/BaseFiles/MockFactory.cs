namespace SweetMock;

/// <summary>
/// Factory for creating mock objects.
/// </summary>
[System.CodeDom.Compiler.GeneratedCode("SweetMock","{{SweetMockVersion}}")]
internal static partial class Mock
{
}

public class ValueBox<T>
{
    public ValueBox(T value)
    {
        Value = value;
    }
    public T Value { get; set; } = default(T);
}
