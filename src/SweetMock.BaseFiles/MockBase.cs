// ReSharper disable RedundantNullableDirective
#nullable enable

using System;
// ReSharper disable VirtualMemberCallInConstructor
// ReSharper disable PartialTypeWithSinglePart

namespace SweetMock;

public class MockBase<TInterface>
{
    public MockOptions Options { get; set; } = new global::SweetMock.MockOptions() {InstanceName = typeof(TInterface).Name};

    public MockConfig Config { get; private set; } = null!;

    public virtual TInterface Value { get; private set; } = default!;

    public partial class MockConfig
    {
        private readonly MockBase<TInterface> target;
        private TInterface? value;

        public static MockConfig Init(MockBase<TInterface> target) =>
            new(target);

        protected MockConfig(MockBase<TInterface> target) =>
            this.target = target;

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

    public MockBase(Action<MockConfig> config, MockOptions options) => this.MockInitialize(config, options);

    public MockBase(TInterface value) => this.MockInitialize(config => config.Value = value);

    public MockBase() => this.MockInitialize(_ => {});

    private bool isInitialized;
    public virtual void MockInitialize(Action<MockConfig>? config = null, MockOptions? options = null)
    {
        if (this.isInitialized)
        {
            throw new TypeInitializationException(typeof(TInterface).Name, null);
        }

        this.isInitialized = true;
        this.Options = options ?? this.Options;
        this.Config = MockConfig.Init(this);
        config?.Invoke(this.Config);
    }
}
