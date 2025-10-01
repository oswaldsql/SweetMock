// ReSharper disable RedundantNameQualifier
// ReSharper disable RedundantUsingDirective
// ReSharper disable RedundantNullableDirective
// ReSharper disable ArrangeNamespaceBody
#nullable enable

using global::System;
using global::System.Collections.Generic;
using global::System.Linq;
using global::System.Runtime.CompilerServices;
using global::System.CodeDom.Compiler;

namespace SweetMock;

using System.Collections;

#region attributes

/// <summary>
/// Instructs SweetMock to create a mock for a specific interface or class.
/// </summary>
/// <typeparam name="T">The type to create a mock based on.</typeparam>
[AttributeUsage(AttributeTargets.All, AllowMultiple = true)]
[GeneratedCode("SweetMock", "{{SweetMockVersion}}")]
internal class MockAttribute<T>(string? mockName = null) : Attribute
{
    public string? MockName { get; } = mockName ?? typeof(T).Name;
}

/// <summary>
/// Specifies that a mock should use a specific custom implementation.
/// </summary>
/// <typeparam name="T">Type of class to mock.</typeparam>
/// <typeparam name="TImplementation">Concrete type of use for mocking.</typeparam>
[AttributeUsage(AttributeTargets.All, AllowMultiple = true)]
[GeneratedCode("SweetMock", "{{SweetMockVersion}}")]
internal class MockAttribute<T, TImplementation> : Attribute
    where TImplementation : MockBase<T>, new()
;

/// <summary>
/// Instructs SweetMock to create a fixture for a specific class.
/// </summary>
/// <typeparam name="T">The type to create a fixture for.</typeparam>
[AttributeUsage(AttributeTargets.All, AllowMultiple = true)]
[GeneratedCode("SweetMock", "{{SweetMockVersion}}")]
internal class FixtureAttribute<T>(string? fixtureName = null) : Attribute where T : class
{
    internal string FixtureName { get; } = fixtureName ?? typeof(T).Name;
}

#endregion

#region CallLog

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

    protected Arguments Arguments { get; private set; } = Arguments.Empty;
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

public interface ICallLog_Filter
{
    public IEnumerable<CallLogItem> Filter();
}

public abstract class CallLog_Filter(IEnumerable<CallLogItem> source) : ICallLog_Filter
{
    protected abstract string SignatureStart { get; }

    IEnumerable<CallLogItem> ICallLog_Filter.Filter() => source.Where(t => t.MethodSignature?.StartsWith(this.SignatureStart) == true);
}

public static class LEX
{
    public static IEnumerable<T> Matching<T>(this IEnumerable<CallLogItem> source, string signature, Func<T, bool>? predicate = null) where T : TypedArguments, new() =>
        source.Where(t => t.MethodSignature == signature)
            .Select(t => new TypedCallLogItem<T>(t).TypedArguments)
            .Where(t => predicate == null || predicate(t));

    public static IEnumerable<T> Matching<T>(this IEnumerable<CallLogItem> source, HashSet<string> signatures, Func<T, bool>? predicate = null) where T : TypedArguments, new() =>
        source.Where(t => signatures.Contains(t.MethodSignature!))
            .Select(t => new TypedCallLogItem<T>(t).TypedArguments)
            .Where(t => predicate == null || predicate(t));
}

#endregion

public class NotExplicitlyMockedException(string memberName, string instanceName) : InvalidOperationException($"'{memberName}' in '{instanceName}' is not explicitly mocked.")
{
    public string MemberName => memberName;

    public string InstanceName => instanceName;
}

public class ValueBox<T>
{
    public ValueBox(T value) =>
        this.Value = value;

    public T Value { get; set; }
}

public class MockOptions
{
    public MockOptions(CallLog? logger = null, string? instanceName = null)
    {
        this.Logger = logger;
        this.InstanceName = instanceName;
    }

    public static MockOptions Default => new();

    public CallLog? Logger { get; init; }

    public string? InstanceName { get; init; }
}
