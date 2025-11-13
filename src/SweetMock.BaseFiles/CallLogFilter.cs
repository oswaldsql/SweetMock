namespace SweetMock;

using System.Collections.Generic;
using System.Linq;

public abstract class CallLogFilter(IEnumerable<CallLogItem> source) : ICallLogFilter
{
    protected abstract string SignatureStart { get; }

    IEnumerable<CallLogItem> ICallLogFilter.Filter() => source.Where(t => t.MethodSignature?.StartsWith(this.SignatureStart) == true);
}
