namespace SweetMock;

public abstract class TypedArguments
{
    internal void Init(Arguments arguments) =>
        this.Arguments = arguments;

    protected Arguments Arguments { get; private set; } = Arguments.Empty;
}