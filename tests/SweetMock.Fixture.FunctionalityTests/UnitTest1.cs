namespace SweetMock.FixtureGenerator.FunctionalityTests;

using Repo;

[SweetMock.Fixture<TestTarget>]
[Fixture<GenericFixtureTarget<TestTarget2>>]
[Mock<GenericMockTarget<TestTarget2>>]
[Mock<IExplicitMock>]
[Mock<ICustomMock, MockOf_ICustomMock>]
public class UnitTest1
{
    /// <summary>
    /// this is a test <see cref="global::Repo.IRepo"/> <see cref="C:System.String"/>
    /// </summary>
    [Fact]
    public void Test1()
    {
        var fix = Fixture.TestTarget(config =>
        {
            config.directValue = "directValue";
            config.imp.ImplicitValue("ImplicitValue");
            config.explicitMock.ExplicitValue("ExplicitValue");
            config.customMock.Value = new CustomMockImplementation();
        });

        var sut = fix.CreateSut();

        Assert.Equal("directValue", sut.GetDirectValue());
        var actual = Assert.Throws<NotExplicitlyMockedException>(() => sut.GetImplicitValue());
        Assert.Equal("'ImplicitMethod' in 'imp' is not explicitly mocked.", actual.Message);
        Assert.Equal("ImplicitMethod", actual.MemberName);
        Assert.Equal("imp", actual.InstanceName);
        
        
        Assert.True(true);

        new MockOf_IRepo2();
        var repo = Mock.IRepo2(config =>
            {
                config.SomeOverload();
            }
            );
    }
}

[Mock<IRepo2>]
public class ClassNamespaceCollisionTests
{
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

public class TestTarget(string directValue, IImplicitMock imp, IExplicitMock explicitMock, ICustomMock customMock)
{
    public string GetDirectValue() => directValue;
    public string GetImplicitValue() => imp.ImplicitMethod();
    public string GetExplicitValue() => explicitMock.ExplicitValue;

    public string GetCustomValue() => customMock.CustomValue;
}

public interface IImplicitMock
{
    public string ImplicitValue { get; set; }
    public string ImplicitMethod();
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

public class Test
{
    [Fact]
    public void METHOD()
    {
        var mockOfICustomMock2 = new MockOf_ICustomMock2();
        // Arrange

        // ACT

        // Assert 
    }
}

internal class MockOf_ICustomMock2() : WrapperMock2<ICustomMock>(new CustomMockImplementation());

internal class WrapperMock2<TInterface>
{
    protected virtual TInterface? value { get; set; } = default(TInterface);

    internal class Config
    {
        private readonly WrapperMock2<TInterface> target;
        private TInterface? value;

        public static Config Init(WrapperMock2<TInterface> target, Action<Config>? config = null)
        {
            var config1 = new Config(target);
            config?.Invoke(config1);
            return config1;
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
        this.Config2 = Config.Init(this, config => config.Value = value);
    }

    public Config Config2 { get; private set; }

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
