// ReSharper disable MemberCanBePrivate.Global
// ReSharper disable ArrangeTypeMemberModifiers

namespace SweetMock.BuilderTests.MemberTypeTests;

using Xunit.Sdk;

public class PropertyTest(ITestOutputHelper testOutputHelper)
{
    [Fact]
    public void PropertyRepositoryTests()
    {
        var source = Build.TestClass<IPropertyInterface>();

        var generate = new SweetMockSourceGenerator().Generate(source);

        testOutputHelper.DumpResult(generate);

        Assert.Empty(generate.GetErrors());
    }

    [Fact]
    public void AbstractClassWithDifferentPropertyTypes()
    {
        var source = Build.TestClass<AbstractClassWithDifferentProperties>();

        var generate = new SweetMockSourceGenerator().Generate(source);

        testOutputHelper.DumpResult(generate);

        Assert.Empty(generate.GetErrors());
    }

    internal interface IPropertyInterface
    {
        string GetSet { get; set; }
        string GetInit { get; init; }
        string? Nullable { get; set; }
        string GetOnly { get; }
        string SetOnly { set; }
    }

    public abstract class AbstractClassWithDifferentProperties
    {
        public string NotAbstract { get; set; } = "";
        public abstract string Abstract { get; set; }
        public virtual string Virtual { get; set; } = "";

        public string NotAbstractGetOnly { get; } = "";
        public abstract string AbstractGetOnly { get; }
        public virtual string VirtualGetOnly { get; } = "";

        public string NotAbstractSetOnly { set => throw new TestClassException("This should not be accessed"); }
        public abstract string AbstractSetOnly { set; }
        public virtual string VirtualSetOnly { set => throw new TestClassException("This should not be accessed"); }
    }
}
