namespace SweetMock;

using System.Collections.Generic;
using System.Linq;

public abstract class CallLog_Filter(IEnumerable<CallLogItem> source) : ICallLog_Filter
{
    protected abstract string SignatureStart { get; }

    IEnumerable<CallLogItem> ICallLog_Filter.Filter() => source.Where(t => t.MethodSignature?.StartsWith(this.SignatureStart) == true);
}