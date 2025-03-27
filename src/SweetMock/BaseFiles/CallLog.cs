#nullable enable
using System.Collections.Generic;
using System.Collections;

namespace SweetMock {
    using System.Linq;

    public class CallLogItem
    {
        public int Index { get; init; }

        public string? MethodSignature { get; init; }

        public Arguments Arguments { get; init; } = Arguments.Empty;

        public override string ToString() => Index.ToString("0000") + " : " + MethodSignature;
    }

    public class TypedCallLogItem<T> : CallLogItem where T : TypedArguments, new()
    {
        public TypedCallLogItem(CallLogItem source)
        {
            base.Index = source.Index;
            base.MethodSignature = source.MethodSignature;
            base.Arguments = source.Arguments;
            TypedArguments = new T();
            this.TypedArguments.Init(source.Arguments);
        }

        public T TypedArguments { get; private set; }
    }

    public abstract class TypedArguments()
    {
        internal void Init(Arguments arguments)
        {
            this.Arguments = arguments;
        }
        
        protected Arguments Arguments { get;private set; } = Arguments.Empty;
    } 
    
    public class Arguments
    {
        Dictionary<string, object?> values = new Dictionary<string, object?>();
        
        public static Arguments Empty => new Arguments();
        
        public static Arguments With(string key, object? value)
        {
            return new Arguments().And(key, value);
        }
        
        public Arguments And(string key, object? value)
        {
            values[key] = value;
            return this;
        }

        public object? this[string key]
        {
            get
            {
                if (values.TryGetValue(key, out var value))
                {
                    return value;
                }
                throw new KeyNotFoundException(key);
            }
        }

        public override string ToString()
        {
            return values.Count == 0 ? "" : string.Join(", ", values.Select(t => $"{t.Key} : '{t.Value}'"));
        }
    }
    
    public class CallLog : System.Collections.Generic.IEnumerable<CallLogItem>
    {
        private int index = 0;
        private List<CallLogItem> _logs = new();
        private object _lock = new();
        public void Add(string signature, Arguments? arguments = null)
        {
            lock (_lock)
            {
                index++;

                if(arguments == null) arguments = Arguments.Empty;
                this._logs.Add(new CallLogItem(){Index = this.index, MethodSignature = signature, Arguments = arguments });
            }
        }

        public IEnumerator<CallLogItem> GetEnumerator() => this._logs.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() =>  this._logs.GetEnumerator();
    }
}