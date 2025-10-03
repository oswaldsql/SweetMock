namespace SweetMock;

public class TypedCallLogItem<T> : CallLogItem where T : TypedArguments, new()
{
    public TypedCallLogItem(CallLogItem source)
    {
        this.Index = source.Index;
        this.MethodSignature = source.MethodSignature;
        this.Arguments = source.Arguments;
        this.TypedArguments = new T();
        this.TypedArguments.Init(source.Arguments);
    }

    public T TypedArguments { get; private set; }
}