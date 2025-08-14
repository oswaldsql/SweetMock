namespace SweetMock.FixtureGenerator.FunctionalityTests;

[SweetMock.Fixture<TestTarget>]
[Fixture<GenericFixtureTarget<TestTarget2>>]
[Mock<GenericMockTarget<TestTarget2>>]
[Mock<IExplicitMock>]
[Mock<ICustomMock, MockOf_ICustomMock>]
public class UnitTest1
{
    [Fact]
    public void Test1()
    {
        var fix = Fixture.TestTarget(config =>
        {
            config.directValue = "TestTarget";
            config.implicitMock.ImplicitValue("TestTarget");
            config.explicitMock.ExplicitValue("TestTarget");
            config.customMock.Value = new CustomMockImplementation();
        });

        var sut = fix.CreateSut();

        Assert.Equal("TestTarget", sut.GetDirectValue());
        
        Assert.True(true);
    }
}

public class GenericMockTarget
    <T>() 
    where T : new()
{
    public T Name { get; set; }
    
    public string GetName() => "test";
}

public class GenericFixtureTarget<T>() where T : new(){};

public class TestTarget2{}

public class TestTarget(string directValue, IImplicitMock implicitMock, IExplicitMock explicitMock, ICustomMock customMock)
{
    public string GetDirectValue() => directValue;
}

public interface IImplicitMock
{
    public string ImplicitValue { get; set; }
}

public interface IExplicitMock
{
    public string ExplicitValue { get; set; }
}

public interface ICustomMock
{
    public string CustomValue { get; set; }
}

public class CustomMockImplementation : ICustomMock
{
    public string CustomValue { get; set; }
}

internal class MockOf_ICustomMock(Action<WrapperMock<ICustomMock>.Config>? config, object? dummy = null) : WrapperMock<ICustomMock>(config)
{
}

internal class MockOf_ICustomMock2() : WrapperMock2<ICustomMock>(new CustomMockImplementation());

internal class WrapperMock2<TInterface>
{
    public static implicit operator TInterface(WrapperMock2<TInterface>  d) => d.Value;

    protected virtual TInterface? value { get; set; } = default(TInterface);

    internal class Config
    {
        private readonly WrapperMock2<TInterface> target;
        private TInterface? value;

        public static void Init(WrapperMock2<TInterface> target, Action<Config>? config = null)
        {
            var config1 = new Config(target);
            config?.Invoke(config1);
        }

        private Config(WrapperMock2<TInterface> target)
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

    public WrapperMock2(TInterface value)
    {
        Config.Init(this, config => config.Value = value);
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
