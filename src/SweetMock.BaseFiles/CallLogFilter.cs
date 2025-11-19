namespace SweetMock;

using System.Collections.Generic;
using System.Linq;

public abstract class CallLogFilter(CallLog source) : ICallLogFilter
{
    protected abstract string SignatureStart { get; }

    public CallLog Filter() => new CallLog(source.ToList(), t => t.MethodSignature?.StartsWith(this.SignatureStart) == true);
}
