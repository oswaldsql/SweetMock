// ReSharper disable ArrangeTypeMemberModifiers
// ReSharper disable MemberCanBePrivate.Global

namespace SweetMock.BuilderTests;

using SweetMock;
using Util;

public class IndexTests(ITestOutputHelper testOutputHelper)
{
    [Fact]
    public void IndexRepositoryTests()
    {
        var source = Build.TestClass<IIndexRepository>();

        var generate = new SweetMockSourceGenerator().Generate(source);

        testOutputHelper.DumpResult(generate);

        Assert.Empty(generate.GetErrors());
    }

    internal interface IIndexRepository
    {
        int this[uint index] { set; }
        int this[int index] { get; }
        int this[string index] { get; set; }
        (string name, int age) this[Guid index] { get; set; }
        string this[(string name, int age) index] { get; set; }
    }
}
