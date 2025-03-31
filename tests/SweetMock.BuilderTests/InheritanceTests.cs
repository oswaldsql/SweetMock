// ReSharper disable ArrangeTypeMemberModifiers
// ReSharper disable MemberCanBePrivate.Global

namespace SweetMock.BuilderTests;

using SweetMock;
using Util;

public class InheritanceTests(ITestOutputHelper testOutputHelper)
{
    [Fact]
    public void SimpleInheritanceTests()
    {
        var source = Build.TestClass<IDerivedM>();

        var generate = new SweetMockSourceGenerator().Generate(source);

        testOutputHelper.DumpResult(generate);

        Assert.Empty(generate.GetWarnings());
    }

    [Fact(Skip = "new keyword not supported yet")]
    public void ClassInheritanceTests2()
    {
        var source = Build.TestClass<IDerived>();

        var generate = new SweetMockSourceGenerator().Generate(source);

        testOutputHelper.DumpResult(generate);

        Assert.Empty(generate.GetWarnings());
    }

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

    [Fact(Skip = "New keyword not working")]
    public void EventInheritanceTests()
    {
        var source = Build.TestClass<IDerivedWithEvent>();

        var generate = new SweetMockSourceGenerator().Generate(source);

        testOutputHelper.DumpResult(generate);

        Assert.Empty(generate.GetWarnings());
    }

    [Fact(Skip = "new keyword not working")]
    public void IndexerInheritanceTests()
    {
        var source = Build.TestClass<IDerivedWithIndexer>();

        var generate = new SweetMockSourceGenerator().Generate(source);

        testOutputHelper.DumpResult(generate);

        Assert.Empty(generate.GetWarnings());
    }

    public interface IBaseM
    {
        bool Method1();
        void Method2();
        bool Method3(string name);
        bool Method4(out string name);
        void Method5(string name);
        string Method6() => "base";
        bool Method7(ref string name);
    }

    public interface IDerivedM : IBaseM
    {
        new bool Method1();
        new void Method2();
        new bool Method3(string name);
        new bool Method4(out string name);
        new void Method5(string name);
        new string Method6() => "Derived ";
        new bool Method7(ref string name);
    }

    public interface IBase
    {
        string Name1 { get; set; }
        string Name2 { set; }
        string Name3 { get; }
    }

    public interface IDerived : IBase
    {
        new string Name1 { get; set; }
        new string Name2 { set; }
        new string Name3 { get; }
    }

    public abstract class Inheritance
    {
        public void Method1() { }
        public virtual void Method2() { }
        public abstract void Method3();
    }

    public interface IBaseWithEvent
    {
        event EventHandler MyEvent;
    }

    public interface IDerivedWithEvent : IBaseWithEvent
    {
        new event EventHandler MyEvent;
    }

    public interface IBaseWithIndexer
    {
        int this[uint index] { set; }
        int this[int index] { get; }
        int this[string index] { get; set; }
    }

    public interface IDerivedWithIndexer : IBaseWithIndexer
    {
        new int this[uint index] { set; }
        new int this[int index] { get; }
        new int this[string index] { get; set; }
    }
}

public class DoubleInheritanceTest(ITestOutputHelper testOutputHelper)
{
    [Fact(Skip = "Multiple inheritance does not work right now")]
    public void MethodsInheritedFromMultipleSourcesShouldOnlyBeWrittenOnce()
    {
        var source = Build.TestClass<IDoubleInheritance>();

        var generate = new SweetMockSourceGenerator().Generate(source);

        testOutputHelper.DumpResult(generate);

        Assert.Empty(generate.GetWarnings());
    }

    // ReSharper disable once RedundantExtendsListEntry (disabled since this is the point of the test)
    internal interface IDoubleInheritance : ICollection<string>, IList<string>
    {
    }
}

public class NullValueTypeTests(ITestOutputHelper testOutputHelper)
{
    [Fact]
    public void NoneNullableValueTypesShouldBePermitted()
    {
        var source = Build.TestClass<INullIntTest>();

        var generate = new SweetMockSourceGenerator().Generate(source);

        testOutputHelper.DumpResult(generate);

        Assert.Empty(generate.GetWarnings());
    }

    public interface INullIntTest
    {
        int IntValue { get; set; }
    }
}
