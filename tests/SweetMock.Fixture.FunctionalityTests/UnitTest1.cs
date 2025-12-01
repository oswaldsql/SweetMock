namespace SweetMock.FixtureGenerator.FunctionalityTests;

using Repo;

[Fixture<TestTarget>]
[Fixture<GenericFixtureTarget<TestTarget2>>]
[Mock<GenericMockTarget<TestTarget2>>]
[Mock<IExplicitMock>]
//[Mock<ICustomMock, MockOfICustomMock>]
public class CreateSutArgumentsTest
{
    [Fact]
    public void Test1()
    {
        var fix = Fixture.TestTarget(config =>
        {
//            config.directValue = "directValue";
            config.imp.ImplicitValue("ImplicitValue");
            config.explicitMock.ExplicitValue("ExplicitValue");
            //config.customMock.Value = new CustomMockImplementation();
        });

        var sut = fix.CreateTestTarget("directValue");

        Assert.Equal("directValue", sut.GetDirectValue());
        var actual = Assert.Throws<NotExplicitlyMockedException>(() => sut.GetImplicitValue());
        Assert.Equal("'ImplicitMethod' in 'imp' is not explicitly mocked.", actual.Message);
        Assert.Equal("ImplicitMethod", actual.MemberName);
        Assert.Equal("imp", actual.InstanceName);

        Assert.True(true);

        fix.Logs.imp.ImplicitValue(arguments => arguments.value == "432");
    }

    [Fact]
    public void NotSettingAValueShouldThrowException()
    {
        // Arrange
        var fixture = Fixture.TestTarget();

        // ACT
        var actual = Record.Exception(() => fixture.CreateTestTarget());

        // Assert 
        Assert.NotNull(actual);
        Assert.IsType<NullReferenceException>(actual);
    }

    [Fact]
    public void SettingTheValueInConfigShouldUseThatValue()
    {
        // Arrange
        var fixture = Fixture.TestTarget(config =>
        {
            config.directValue = "from config";
        });
        var sut = fixture.CreateTestTarget();
        
        // ACT
        var actual = sut.GetDirectValue();

        // Assert 
        Assert.Equal("from config", actual);
    }
    
    [Fact]
    public void SettingTheValueInCreateShouldUseThatValue()
    {
        // Arrange
        var fixture = Fixture.TestTarget();
        var sut = fixture.CreateTestTarget("from create");
        
        // ACT
        var actual = sut.GetDirectValue();

        // Assert 
        Assert.Equal("from create", actual);
    }

    [Fact]
    public void WhenSettingTheValueInBothLocationsTheCreateShouldWin()
    {
        // Arrange
        var fixture = Fixture.TestTarget(config =>
        {
            config.directValue = "from config";
        });
        var sut = fixture.CreateTestTarget("from create");
        
        // ACT
        var actual = sut.GetDirectValue();

        // Assert 
        Assert.Equal("from create", actual);
    }
}

[Mock<IRepo2>]
public class ClassNamespaceCollisionTests
{
}

public class GenericMockTarget
    <T> where T : new()
{
    public T Name { get; set; } = new();

    public string GetName()
    {
        return "test";
    }
}

public class GenericFixtureTarget<T> where T : new()
{
};

public class TestTarget2
{
}

public class TestTarget(string directValue, IImplicitMock imp, IExplicitMock explicitMock, ICustomMock customMock)
{
    public string GetDirectValue()
    {
        return directValue;
    }

    public string GetImplicitValue()
    {
        return imp.ImplicitMethod();
    }

    public string GetExplicitValue()
    {
        return explicitMock.ExplicitValue;
    }

    public string GetCustomValue()
    {
        return customMock.CustomValue;
    }
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
    public string CustomValue { get; set; } = "";
}

//internal class MockOfICustomMock() : MockBase<ICustomMock>(new CustomMockImplementation());

internal class WrapperMock2<TInterface>
{
    public WrapperMock2(TInterface value)
    {
        Config2 = Config.Init(this, config => config.Value = value);
    }

    protected virtual TInterface? value { get; set; }

    public Config Config2 { get; private set; }

    internal TInterface Value
    {
        get => value!;
        private set => this.value = value;
    }

    internal class Config
    {
        private readonly WrapperMock2<TInterface> target;
        private TInterface? value;

        private Config(WrapperMock2<TInterface> target)
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

        public static Config Init(WrapperMock2<TInterface> target, Action<Config>? config = null)
        {
            var config1 = new Config(target);
            config?.Invoke(config1);
            return config1;
        }
    }
}