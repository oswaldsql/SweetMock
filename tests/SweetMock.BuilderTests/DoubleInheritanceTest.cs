namespace SweetMock.BuilderTests;

using Util;

public class DoubleInheritanceTest(ITestOutputHelper testOutputHelper)
{
    [Fact]
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