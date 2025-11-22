namespace SweetMock;

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class CallLog(List<CallLogItem>? source = null, Func<CallLogItem, bool>? filter = null) : IEnumerable<CallLogItem>
{
    private readonly Func<CallLogItem, bool> filter = filter ?? (_ => true);
    private readonly object @lock = new();
    private readonly List<CallLogItem> logs = source ?? [];
    private int index;

    public IEnumerator<CallLogItem> GetEnumerator() => this.logs.Where(this.filter).GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator() => this.GetEnumerator();

    public void Add(string signature, Arguments? arguments = null)
    {
        lock (this.@lock)
        {
            this.index++;

            arguments ??= Arguments.Empty;

            this.logs.Add(new() { Index = this.index, MethodSignature = signature, Arguments = arguments });
        }
    }

    public IEnumerable<T> Matching<T>(string signature, Func<T, bool>? predicate = null) where T : TypedArguments, new() =>
        this.logs.Where(t => t.MethodSignature == signature && this.filter(t))
            .Select(t => new TypedCallLogItem<T>(t).TypedArguments)
            .Where(t => predicate == null || predicate(t));

    public IEnumerable<T> Matching<T>(HashSet<string> signatures, Func<T, bool>? predicate = null) where T : TypedArguments, new() =>
        this.logs.Where(t => signatures.Contains(t.MethodSignature!) && this.filter(t))
            .Select(t => new TypedCallLogItem<T>(t).TypedArguments)
            .Where(t => predicate == null || predicate(t));
}
