namespace SweetMock.BuilderTests;

public class ConfigClassTests(ITestOutputHelper testOutputHelper)
{
    [Fact]
    public void EmptyInterfaceTests()
    {
        var source = Build.TestClass<IEmptyInterface>();

        var generate = new SweetMockSourceGenerator().Generate(source);

        testOutputHelper.DumpResult(generate);

        Assert.Empty(generate.GetErrors());
    }

    [Fact]
    public void EmptyClassTests()
    {
        var source = Build.TestClass<EmptyClass>();

        var generate = new SweetMockSourceGenerator().Generate(source);

        testOutputHelper.DumpResult(generate);

        Assert.Empty(generate.GetErrors());
    }

    [Fact]
    public void AbstractClassTests()
    {
        var source = Build.TestClass<AbstractClass>();

        var generate = new SweetMockSourceGenerator().Generate(source);

        testOutputHelper.DumpResult(generate);

        Assert.Empty(generate.GetErrors());
    }

    internal interface IEmptyInterface
    {
    }

    internal class EmptyClass
    {
    }

    internal sealed class SealedClass
    {
    }

    internal abstract class AbstractClass
    {
    }
}
