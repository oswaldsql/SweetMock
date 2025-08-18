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
    public MockOptions(CallLog? logger = null, string? instanceName = null)
    {
        this.Logger = logger;
        this.InstanceName = instanceName;
    }

    public static MockOptions Default => new();

    public CallLog? Logger { get; init; } = null;

    public string? InstanceName  { get; init; } = null;
}

internal class WrapperMock<TInterface>
{
    public MockOptions? Options { get; }
    protected virtual TInterface? value { get; set; } = default(TInterface);

    internal class MockConfig
    {
        private readonly WrapperMock<TInterface> target;
        private TInterface? value;

        public static void Init(WrapperMock<TInterface> target, Action<MockConfig>? config = null)
        {
            var config1 = new MockConfig(target);
            config?.Invoke(config1);
        }

        private MockConfig(WrapperMock<TInterface> target)
        {
            this.target = target;
        }

        public TInterface Value
        {
            get => this.value ?? throw new NullReferenceException();
            set
            {
                this.target.Value = value;
                this.value = value;
            }
        }
    }

    public WrapperMock(Action<MockConfig>? config, MockOptions? options)
    {
        this.Options = options;
        MockConfig.Init(this, config);
    }

    public WrapperMock(TInterface value)
    {
        MockConfig.Init(this, config => config.Value = value);
    }

    internal TInterface Value
    {
        get
        {
            return value!;
        }
        private set
        {
            this.value = value;
        }
    }
}
