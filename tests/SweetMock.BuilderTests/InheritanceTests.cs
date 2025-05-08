// ReSharper disable ArrangeTypeMemberModifiers
// ReSharper disable MemberCanBePrivate.Global

namespace SweetMock.BuilderTests;

public class InheritanceTests(ITestOutputHelper testOutputHelper)
{
    [Fact]
    public void ClassInheritanceTests()
    {
        var source = Build.TestClass<Inheritance>();

        var generate = new SweetMockSourceGenerator().Generate(source);

        testOutputHelper.DumpResult(generate);

        Assert.Empty(generate.GetWarnings());
        var file = Assert.Single(generate.GetFileContent(".Base.g.cs"));
        Assert.DoesNotContain("void Method1()", file);
        Assert.Contains("void Method2()", file);
        Assert.Contains("void Method3()", file);
    }

    public abstract class Inheritance
    {
        public void Method1() { }
        public virtual void Method2() { }
        public abstract void Method3();
    }
}