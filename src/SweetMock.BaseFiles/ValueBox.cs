namespace SweetMock;

using System.ComponentModel;

[EditorBrowsable(EditorBrowsableState.Never)]
public class ValueBox<T>
{
    public ValueBox(T value) =>
        this.Value = value;

    public T Value { get; set; }
}
