#nullable enable
using System.Collections;

namespace SweetMock {
    public class CallLogItem
    {
        public int Index { get; init; }

        public string? MethodSignature { get; init; }

        public Arguments Arguments { get; init; } = Arguments.Empty;

        public override string ToString() => this.Index.ToString("0000") + " : " + this.MethodSignature;
    }

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

    public abstract class TypedArguments
    {
        internal void Init(Arguments arguments) =>
            this.Arguments = arguments;

        protected Arguments Arguments { get;private set; } = Arguments.Empty;
    }

    public class Arguments
    {
        private readonly Dictionary<string, object?> values = new Dictionary<string, object?>();

        public static Arguments Empty => new Arguments();

        public static Arguments With(string key, object? value) =>
            new Arguments().And(key, value);

        public Arguments And(string key, object? value)
        {
            this.values[key] = value;
            return this;
        }

        public object? this[string key]
        {
            get
            {
                if (this.values.TryGetValue(key, out var value))
                {
                    return value;
                }
                throw new KeyNotFoundException(key);
            }
        }

        public override string ToString() =>
            this.values.Count == 0 ? "" : string.Join(", ", this.values.Select(t => $"{t.Key} : '{t.Value}'"));
    }

    public class CallLog
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

                this.logs.Add(new CallLogItem(){Index = this.index, MethodSignature = signature, Arguments = arguments });
            }
        }

        public IEnumerable<TypedCallLogItem<T>> Matching<T>(string signature, Func<T, bool>? predicate = null) where T : TypedArguments, new() =>
            this.logs.Where(t => t.MethodSignature == signature)
                .Select(t => new TypedCallLogItem<T>(t))
                .Where(t => predicate == null || predicate(t.TypedArguments));

        public IEnumerable<TypedCallLogItem<T>> Matching<T>(HashSet<string> signatures, Func<T, bool>? predicate = null) where T : TypedArguments, new() =>
            this.logs.Where(t => signatures.Contains(t.MethodSignature!))
                .Select(t => new TypedCallLogItem<T>(t))
                .Where(t => predicate == null || predicate(t.TypedArguments));
    }
}
