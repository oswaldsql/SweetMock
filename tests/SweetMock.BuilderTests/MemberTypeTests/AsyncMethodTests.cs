// ReSharper disable ArrangeTypeMemberModifiers
// ReSharper disable MemberCanBePrivate.Global

namespace SweetMock.BuilderTests.MemberTypeTests;

public class AsyncMethodTests(ITestOutputHelper testOutputHelper)
{
    [Fact]
    public void SimpleTaskMethodsTests()
    {
        var source = Build.TestClass<ITaskMethods>();

        var generate = new SweetMockSourceGenerator().Generate(source);

        testOutputHelper.DumpResult(generate);

        Assert.Empty(generate.GetWarnings());
    }

    [Fact]
    public void GenericTaskMethodsTests()
    {
        var source = Build.TestClass<IValueTaskMethods>();

        var generate = new SweetMockSourceGenerator().Generate(source);

        testOutputHelper.DumpResult(generate);

        Assert.Empty(generate.GetWarnings());
    }

    [Fact]
    public void CanCreateMockForINotifyPropertyChanged()
    {
        var source = Build.TestClass<INotifyPropertyChanged>();

        var generate = new SweetMockSourceGenerator().Generate(source);

        testOutputHelper.DumpResult(generate);

        Assert.Empty(generate.GetWarnings());
    }

    public interface ITaskMethods
    {
        Task WithParameter(string name);
        Task WithoutParameter();

        Task<string> GenericWithParameter(string name);
        Task<int> GenericWithoutParameter();
    }

    public interface IValueTaskMethods
    {
        ValueTask WithParameter(string name);
        ValueTask WithoutParameter();
        ValueTask<string> GenericWithParameter(string name);
        ValueTask<int> GenericWithoutParameter();
    }
}