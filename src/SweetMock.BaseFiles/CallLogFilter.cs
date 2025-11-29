namespace SweetMock;

using System.Linq;

public abstract class CallLogFilter(CallLog source, string signature)
{
    public CallLog Filter() => new(source.ToList(), t => t.MethodSignature?.StartsWith(signature) == true);
}
