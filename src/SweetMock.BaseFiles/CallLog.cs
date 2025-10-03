namespace SweetMock;

using System.Collections;
using System.Collections.Generic;

public class CallLog : IEnumerable<CallLogItem>
{
    private int index;
    private readonly List<CallLogItem> logs = new();
    private readonly object @lock = new();

    public void Add(string signature, Arguments? arguments = null)
    {
        lock (this.@lock)
        {
            this.index++;

            arguments ??= Arguments.Empty;

            this.logs.Add(new CallLogItem() { Index = this.index, MethodSignature = signature, Arguments = arguments });
        }
    }

    public IEnumerator<CallLogItem> GetEnumerator() => this.logs.GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator() => this.GetEnumerator();
}