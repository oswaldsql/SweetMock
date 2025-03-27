// ReSharper disable ArrangeTypeMemberModifiers
// ReSharper disable MemberCanBePrivate.Global

namespace SweetMock.BuilderTests;

using SweetMock;
using Util;

public class OutArgumentTests(ITestOutputHelper testOutputHelper)
{
    [Fact]
    public void MethodWithOutArgumentTests()
    {
        var source = Build.TestClass<IMethodWithOutArgument>();

        var generate = new SweetMockSourceGenerator().Generate(source);

        testOutputHelper.DumpResult(generate);

        Assert.Empty(generate.GetErrors());
    }

    public interface IMethodWithOutArgument
    {
        bool OutWithReturn(string s, out int value);
        void OutWithVoid(string s, out int value);
        int WithRef(string s, ref int value);
    }
}
