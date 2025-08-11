#nullable enable
namespace SweetMock;

using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;

/// <summary>
/// Factory for creating mock objects.
/// </summary>
[System.CodeDom.Compiler.GeneratedCode("SweetMock","{{SweetMockVersion}}")]
internal static partial class Mock { }

/// <summary>
/// Factory for creating test fixtures.
/// </summary>
[System.CodeDom.Compiler.GeneratedCode("SweetMock","{{SweetMockVersion}}")]
internal static partial class Fixture{}

public class ValueBox<T>
{
    public ValueBox(T value) =>
        this.Value = value;
    public T Value { get; set; }
}

public class MockOptions
{
    public MockOptions(CallLog? logger = null, string? instanceName = null) =>
        this.Logger = logger;

    public static MockOptions Default => new();

    public CallLog? Logger { get; init; } = null;

    public string? InstanceName  { get; init; } = null;
}

internal class WrapperMock<TInterface>
{
    public static implicit operator TInterface(WrapperMock<TInterface>  d) => d.Value;

    private bool valueIsSet = false;
    protected virtual TInterface? value { get; set; } = default(TInterface);

    internal class Config
    {
        private readonly WrapperMock<TInterface> target;
        private TInterface? value;

        public static void Init(WrapperMock<TInterface> target, Action<Config>? config = null)
        {
            var config1 = new Config(target);
            config?.Invoke(config1);
        }

        private Config(WrapperMock<TInterface> target)
        {
            this.target = target;
        }

        public TInterface Value
        {
            get => value ?? throw new NullReferenceException();
            set
            {
                target.Value = value;
                this.value = value;
            }
        }
    }

    public WrapperMock(Action<Config>? config)
    {
        Config.Init(this, config);
    }

    public WrapperMock(TInterface value)
    {
        Config.Init(this, config => config.Value = value);
    }

    internal TInterface Value
    {
        get
        {
            if(valueIsSet)
                return value!;
            throw new Exception("Value is not set");
        }
        private set
        {
            this.value = value;
            valueIsSet = true;
        }
    }
}
